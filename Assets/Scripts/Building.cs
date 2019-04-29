using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Building : MonoBehaviour
{
    public BuildingType Type;
    public float WorkLeft = 9999;
    public List<Worker> Workers = new List<Worker>();
    public float WorkerTimer = 15;
    public float ProduceTimer = 10;
    public readonly object Locker = new object();
    private bool _buildingStarted;
    private AudioSource _audioSource;
    public bool PreBuilt = false;
    private bool _buildingFinished;

    void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        if (!PreBuilt)
            _audioSource.PlayOneShot(SoundManager.BuildStarted);
    }

    public void ChangeMaterial()
    {
        Material m = Resources.Load("Materials/" + Type.Name, typeof(Material)) as Material;
        transform.Find("Model").gameObject.GetComponent<Renderer>().material = m;
    }

    private void FixedUpdate()
    {
        if (!_audioSource.isPlaying && _buildingStarted && WorkLeft > 0)
        {
            _audioSource.PlayOneShot(SoundManager.Build.OrderBy(x => Random.value).First(), 0.3f);
        }
    }

    void Update()
    {
        // update and kill
        lock (Locker)
        {
            if (!_buildingStarted && Workers.Count > 0 && Workers.All(x => x.Arrived))
            {
                _buildingStarted = true;
                foreach (Worker worker in Workers)
                {
                    worker.gameObject.transform.LookAt(transform.position);
                    Animator animator = worker.gameObject.GetComponent<Animator>();
                    animator.SetBool(Worker.IsWalking, false);
                    animator.SetBool(Worker.IsBuilding, true);
                }
                
            }
            
            if (_buildingStarted && !_buildingFinished)
            {
                if (WorkLeft <= 0)
                {
                    _buildingFinished = true;
                    ChangeMaterial();
                    _audioSource.PlayOneShot(SoundManager.BuildFinished);
                    foreach (Worker worker in Workers)
                    {
                        GameObject workerGameObject = worker.gameObject;
                        workerGameObject.GetComponent<Worker>().Kill();
                    }
                    Workers.Clear();
                    if (Type.Name == "Attraction")
                    {
                        lock (GameLoop.AttractionLocker)
                        {
                            GameLoop.Attractions.Add(gameObject);
                        }
                    }
                }
                else
                    WorkLeft -= Time.deltaTime;
            }
        }
        
        // generate
        if (WorkLeft <= 0)
        {
            if (Type.Name == "House")
            {
                if (WorkerTimer <= 0)
                {
                    // generate
                    var position = transform.position;
                    var worker =
                        (GameObject) Instantiate(GameLoop.Prefabs["Worker"], position + new Vector3(5, 0, 0), Quaternion.identity);
                    lock (GameLoop.Locker)
                    {
                        GameLoop.Workers.Add(worker);
                    }
                    var randX = Random.value * 9.8f + 5;
                    var randZ = Random.value * 9.6f - 4.8f;
                    worker.GetComponent<NavMeshAgent>().destination = position + new Vector3(randX, 0, randZ);
                    worker.GetComponent<Animator>().SetBool(Worker.IsWalking, true);

                    lock (GameLoop.ScreenLocker)
                    {
                        WorkerTimer = (int) Mathf.Ceil(15 * (1 - GameLoop.Cohesion * 0.006f));
                    }
                }
                else
                {
                    WorkerTimer -= Time.deltaTime;
                }
            }
            
            if (ProduceTimer <= 0)
            {
                lock (GameLoop.ScreenLocker)
                {
                    GameLoop.Cohesion += Type.Cohesion;
                    if (GameLoop.Cohesion < 0) GameLoop.Cohesion = 0;
                    if (GameLoop.Cohesion > 100) GameLoop.Cohesion = 100;
                    GameLoop.Stability += Type.Stability;
                    if (GameLoop.Stability < 0) GameLoop.Stability = 0;
                    if (GameLoop.Stability > 100) GameLoop.Stability = 100;
                    GameLoop.MoneyPower += Type.MoneyPower;
                    if (GameLoop.MoneyPower < 0) GameLoop.MoneyPower = 0;
                    if (GameLoop.MoneyPower > 100) GameLoop.MoneyPower = 100;
                }
                
                ProduceTimer = 10;
            }
            else
            {
                ProduceTimer -= Time.deltaTime;
            }
        }
    }
}