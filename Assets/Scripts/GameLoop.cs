using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class GameLoop : MonoBehaviour
{
    private static float _year = 1948;
    public static float Cohesion = 10;
    public static float MoneyPower = 10;
    public static float Stability = 80;
    public static int LargestPopulation = 0;
    public static int LargestWorkerCount = 0;

    private Vector3 _camPos;
    
    public static readonly object Locker = new object();
    public static readonly object ScreenLocker = new object();
    public static readonly object AttractionLocker = new object();
    public static readonly object ScoreLocker = new object();

    public static int Captured = 0;
    public static int Killed = 0;
    
    public static readonly Dictionary<string, Object> Prefabs = new Dictionary<string, Object>();
    private Camera _camera;
    private AudioSource _audioSource;

    private Text _screenText;
    private GameObject _tooltip;
    private Text _tooltipText;
    private Button _resignButton;
    
    private GameObject _currentHolding;
    private BuildingType _currentBuildingType;
    
    private readonly Rect _boundary = new Rect(-50, -50, 100, 100);

    private static readonly Dictionary<int, bool> GameMap = new Dictionary<int, bool>();
    public static readonly List<GameObject> Workers = new List<GameObject>();
    public static readonly List<GameObject> Attractions = new List<GameObject>();

    private Material _blueprint;
    private static readonly int WireColor = Shader.PropertyToID("_WireColor");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        // generate building types
        BuildingType.Create("House", 10, 6, 0.1f, 0, 0.2f, 2);
        BuildingType.Create("Factory", 20, 15, 0.3f, 0.3f, -0.2f, 2, 2);
        BuildingType.Create("RocketLaunchBase", 50, 50, 0.1f, -0.5f, -0.2f, 3, 3);
        BuildingType.Create("NuclearTestSite", 120, 100, -0.1f, 1f, -0.5f, 4, 4);
        BuildingType.Create("BronzeStatue", 30, 30, 0.1f, -0.5f, 0.5f, 3, 3);
        BuildingType.Create("Attraction", 15, 10, 0.1f, 0.5f, 0);
        
        _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        _audioSource = _camera.gameObject.GetComponent<AudioSource>();
        _screenText = GameObject.Find("TextGUI").GetComponent<Text>();
        _tooltip = GameObject.Find("Tooltip");
        _tooltipText = GameObject.Find("TooltipText").GetComponent<Text>();
        _resignButton = GameObject.Find("ButtonQuit").GetComponent<Button>();
        _resignButton.onClick.AddListener(delegate { SceneManager.LoadScene("EndScene"); });
        _tooltip.SetActive(false);
        
        Prefabs["Worker"] = Resources.Load("Prefabs/Worker");
        Prefabs["House"] = Resources.Load("Prefabs/House");
        Prefabs["Factory"] = Resources.Load("Prefabs/Factory");
        Prefabs["RocketLaunchBase"] = Resources.Load("Prefabs/RocketLaunchBase");
        Prefabs["NuclearTestSite"] = Resources.Load("Prefabs/NuclearTestSite");
        Prefabs["BronzeStatue"] = Resources.Load("Prefabs/BronzeStatue");
        Prefabs["Attraction"] = Resources.Load("Prefabs/Attraction");
        
        _blueprint = Resources.Load("Materials/Blueprint", typeof(Material)) as Material;
        
        Dictionary<string, string> tooltips = new Dictionary<string, string>
        {
            {
                "House", "House - 6 workers\n\nProduces a worker every few seconds, based on your Cohesion.\n\nEvery 10 seconds:\nCohesion +0.1\nStability +0.2"
            },
            {
                "Attraction", "Attraction - 10 workers\n\nAttracts visitors based on your GDP.\n\nEvery 10 seconds:\nCohesion +0.1\nGDP +0.5"
            },
            {
                "Factory", "Factory - 15 workers\n\nEvery 10 seconds:\nCohesion +0.3\nGDP +0.3\nStability -0.2"
            },
            {
                "NuclearTestSite", "Nuclear Site - 100 workers\n\nEvery 10 seconds:\nCohesion -0.1\nGDP +1\nStability: -0.5"
            },
            {
                "BronzeStatue", "Bronze Statue - 30 workers\n\nEvery 10 seconds:\nCohesion +0.1\nGDP -0.5\nStability: +0.5"
            },
            {
                "RocketLaunchBase", "Rocket Base - 50 workers\n\nEvery 10 seconds:\nCohesion +0.1\nGDP -0.5\nStability -0.2"
            },
        };
        
        List<Button> buttons = new List<Button>();
        GameObject.Find("ButtonPanel").GetComponentsInChildren(buttons);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(delegate { ButtonOnClick(button); });
            
            EventTrigger eventTrigger = button.gameObject.AddComponent<EventTrigger>();
            
            EventTrigger.Entry eventEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            eventEntry.callback.AddListener((data) =>
            {
                _tooltipText.text = tooltips[button.gameObject.name];
                _tooltip.SetActive(true);
            });
            eventTrigger.triggers.Add(eventEntry);
            
            eventEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            eventEntry.callback.AddListener((data) => { _tooltip.SetActive(false); });
            eventTrigger.triggers.Add(eventEntry);
        }

        BuildHouse(-45, 45);
        BuildHouse(-45, 35);

        lock (Locker)
        {
            for (int i = 0; i < 30; ++i)
            {
                var randX = Random.value * 20f - 10f;
                var randZ = Random.value * 20f - 10f;
                GameObject worker =
                    (GameObject) Instantiate(Prefabs["Worker"], new Vector3(randX, 0, randZ), Quaternion.identity);
                Workers.Add(worker);
            }
        }
    }

    private void BuildHouse(int x, int z)
    {
        for (int w = x; w < x + 2 * 10; w += 10)
        {
            for (int h = z; h < z + 1 * 10; h += 10)
            {
                GameMap[w * 10000 + h] = true;
            }
        }
        
        var obj = (GameObject) Instantiate(Prefabs["House"], new Vector3(x, 0, z), Quaternion.identity);
        Building cb = obj.AddComponent<Building>();
        cb.PreBuilt = true;
        cb.Type = BuildingType.List["House"];
        cb.WorkLeft = 0;
        cb.ChangeMaterial();
    }
    
    private void ButtonOnClick(Button button)
    {
        _audioSource.PlayOneShot(SoundManager.ButtonClick);
        if (_currentHolding != null)
            Destroy(_currentHolding);
        string buildingTypeName = button.gameObject.name;
        _currentHolding =
            (GameObject) Instantiate(Prefabs[buildingTypeName], new Vector3(0, 9999, 0), Quaternion.identity);
        _currentHolding.transform.Find("Model").gameObject.GetComponent<Renderer>().material = Instantiate(_blueprint);
        _currentBuildingType = BuildingType.List[buildingTypeName];
    }

    private void SendToPosition(GameObject worker, Building cb, Vector3 pos)
    {
        NavMeshAgent a = worker.GetComponent<NavMeshAgent>();
        a.radius = 0.1f;
        a.stoppingDistance = 0.2f;
        Worker w = worker.GetComponent<Worker>();
        w.Arrived = false;
        lock (cb.Locker)
        {
            cb.Workers.Add(w);
        }
        worker.GetComponent<NavMeshAgent>().destination = pos;
        worker.GetComponent<Animator>().SetBool(Worker.IsWalking, true);
    }

    void SetText()
    {
        lock (ScreenLocker)
        {
            int workerCount = Workers.Count;
            int population = GameObject.FindGameObjectsWithTag("Worker").Length;

            if (workerCount > LargestWorkerCount)
                LargestWorkerCount = workerCount;
            if (population > LargestPopulation)
                LargestPopulation = population;
            
            _screenText.text = $"Year: {_year:0}\nWorkers: {workerCount}\nPopulation: {population}" +
                               $"\nCohesion: {Cohesion:0.##}\nGDP: {MoneyPower:0.##}\nStability: {Stability:0.##}";
        }
    }

    private void FixedUpdate()
    {
        _year += Time.fixedDeltaTime / 10f;
        if (_year >= 2019)
            SceneManager.LoadScene("EndScene");
        
        SetText();

        lock (ScreenLocker)
        {
            if (Attractions.Count > 0)
            {
                float possibility = Time.fixedDeltaTime * MoneyPower / 600f;
                if (Random.value < possibility)
                {
                    Vector3 pos = Attractions.OrderBy(x => Random.value).First().transform.position;
                    float randX = Random.value * 9.6f - 4.8f;
                    float randZ = Random.value * 9.6f - 4.8f;
                    var worker =
                        (GameObject) Instantiate(Prefabs["Worker"], pos + new Vector3(randX, 0, randZ), Quaternion.identity);
                    worker.GetComponent<Worker>().IsVisitor = true;
                }
            }

            if (Workers.Count > 0)
            {
                float possibility = Time.fixedDeltaTime * (1 - Stability) / 500f;
                if (Random.value < possibility)
                {
                    lock (Locker)
                    {
                        Workers[0].GetComponent<Worker>().SetDefector();
                        Workers.RemoveAt(0);
                    }
                }
            }
        }
    }

    void Update()
    {
        List<Button> buttons = new List<Button>();
        GameObject.Find("ButtonPanel").GetComponentsInChildren(buttons);
        lock (Locker)
        {
            foreach (Button button in buttons)
            {
                BuildingType bt = BuildingType.List[button.gameObject.name];
                button.interactable = Workers.Count >= bt.WorkersNeeded;
            }
        }
        
        var mousePos = Input.mousePosition;
        
        if (Input.GetMouseButtonDown(2))
        {
            _camPos = mousePos;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 offset = _camera.ScreenToViewportPoint(_camPos - mousePos);
            Vector3 move = new Vector3(offset.x * 60f, 0, offset.y * 60f);
            _camera.transform.Translate(move, Space.World);
            _camPos = mousePos;
        }

        float camFov = _camera.fieldOfView;
        camFov += Input.GetAxis("Mouse ScrollWheel") * -20f;
        camFov = Mathf.Clamp(camFov, 20, 90);
        _camera.fieldOfView = camFov;
        
        Ray ray = _camera.ScreenPointToRay(mousePos);

        if (_currentHolding == null || _currentBuildingType == null)
        {
            // kill
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObject = hit.transform.gameObject;
                Worker w = hitObject.GetComponent<Worker>();
                if (w != null && Input.GetMouseButtonDown(0))
                {
                    if (w.IsVisitor)
                    {
                        w.CaptureVisitor();
                    }
                    else if (w.IsDefector)
                    {
                        w.KillDefector();
                    }
                }
            }
        }
        else
        {
            int width = _currentBuildingType.Width;
            int height = _currentBuildingType.Height;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var followMousePos = hit.point;

                followMousePos.x = Mathf.Clamp(followMousePos.x - 5f, _boundary.x, _boundary.x + _boundary.width - 10f);
                followMousePos.z = Mathf.Clamp(followMousePos.z - 5f, _boundary.y, _boundary.y + _boundary.height - 10f);
                    
                followMousePos.x = Mathf.Round(followMousePos.x / 10f) * 10f + 5f;
                followMousePos.z = Mathf.Round(followMousePos.z / 10f) * 10f + 5f;
                followMousePos.y = 0;
                
                _currentHolding.transform.position = followMousePos;
            }

            lock (Locker)
            {
                bool collided = false;
                var currentPos = _currentHolding.transform.position;
                for (int w = (int)currentPos.x + 0; w < (int)currentPos.x + width * 10; w += 10)
                {
                    for (int h = (int)currentPos.z + 0; h < (int)currentPos.z + height * 10; h += 10)
                    {
                        if (GameMap.ContainsKey(w * 10000 + h) && GameMap[w * 10000 + h])
                        {
                            collided = true;
                            break;
                        }
                    }

                    if (collided)
                        break;
                }

                if (Workers.Count < _currentBuildingType.WorkersNeeded || currentPos.x > 45 - (width - 1) * 10 || currentPos.z < -45 + (height - 1) * 10)
                    collided = true;
                
                Color c = collided ? Color.red : Color.green;
                Material mat = _currentHolding.transform.Find("Model").gameObject.GetComponent<Renderer>().material;
                mat.SetColor(WireColor, c);
                mat.SetColor(BaseColor, c);
                
                if (Input.GetMouseButton(1))
                {
                    _audioSource.PlayOneShot(SoundManager.Deselect);
                    Destroy(_currentHolding);
                    _currentHolding = null;
                    _currentBuildingType = null;
                }
                else if (Input.GetMouseButtonDown(0) && _currentHolding.transform.position.y.Equals(0) && !collided)
                {
                    for (int w = (int)currentPos.x + 0; w < (int)currentPos.x + width * 10; w += 10)
                    {
                        for (int h = (int)currentPos.z + 0; h > (int)currentPos.z - height * 10; h -= 10)
                        {
                            GameMap[w * 10000 + h] = true;
                        }
                    }

                    Building cb = _currentHolding.AddComponent<Building>();
                    cb.Type = _currentBuildingType;
                    cb.WorkLeft = _currentBuildingType.AmountOfWork;
                    
                    float realWidth = width * 10;
                    float realHeight = height * 10;
                    var startPoint = _currentHolding.transform.position - new Vector3(4.8f, 0, -4.8f);
                    
                    SendToPosition(Workers[0], cb, startPoint);
                    SendToPosition(Workers[1], cb, startPoint + new Vector3(realWidth - 0.4f, 0, 0));
                    SendToPosition(Workers[2], cb, startPoint + new Vector3(realWidth - 0.4f, 0, -realHeight + 0.4f));
                    SendToPosition(Workers[3], cb, startPoint + new Vector3(0, 0, -realHeight + 0.4f));

                    float totalLength = 2 * realWidth + 2 * realHeight;
                    float distanceBetween = totalLength / _currentBuildingType.WorkersNeeded;
                    float lTop = realWidth - 0.4f;
                    float lRight = realHeight - 0.4f;
                    float lBottom = realWidth - 0.4f;
                    float lLeft = realHeight - 0.4f;
                    for (int i = 4; i < _currentBuildingType.WorkersNeeded; ++i)
                    {
                        if (lTop > distanceBetween)
                        {
                            lTop -= distanceBetween;
                            SendToPosition(Workers[i], cb, startPoint + new Vector3(realWidth - 0.4f - lTop, 0, 0));
                        }
                        else if (lRight > distanceBetween)
                        {
                            lRight -= distanceBetween;
                            SendToPosition(Workers[i], cb, startPoint + new Vector3(realWidth - 0.4f, 0, -(realHeight - 0.4f - lRight)));
                        }
                        else if (lBottom > distanceBetween)
                        {
                            lBottom -= distanceBetween;
                            SendToPosition(Workers[i], cb, startPoint + new Vector3(0.2f + lBottom, 0, -(realHeight - 0.4f)));
                        }
                        else
                        {
                            lLeft -= distanceBetween;
                            SendToPosition(Workers[i], cb, startPoint + new Vector3(0.2f, 0, -(realHeight - 0.4f - lLeft)));
                        }
                    }
                    
                    Workers.RemoveRange(0, _currentBuildingType.WorkersNeeded);
                    _currentHolding = null;
                    _currentBuildingType = null;
                }
            }
        }
    }
}
