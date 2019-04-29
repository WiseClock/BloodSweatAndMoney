using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Color = UnityEngine.Color;

public class Worker : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;
    public static readonly int IsWalking = Animator.StringToHash("IsWalking");
    public static readonly int IsBuilding = Animator.StringToHash("IsBuilding");
    public bool Arrived;

    public bool IsDefector;
    public bool IsVisitor;
    private static readonly int Color = Shader.PropertyToID("_Color");
    public float WorkerTimer = 2;
    private AudioSource _audioSource;

    private void Start()
    {
        GameObject o = gameObject;
        _agent = o.GetComponent<NavMeshAgent>();
        _animator = o.GetComponent<Animator>();
        o.GetComponent<Collider>().enabled = false;
        _audioSource = o.AddComponent<AudioSource>();
        
        if (IsVisitor)
            SetVisitor();
    }

    public void Kill()
    {
        gameObject.transform.Find("Mesh1").GetComponent<Renderer>().enabled = false;
        _audioSource.clip = SoundManager.Shout.OrderBy(x => Random.value).First();
        _audioSource.volume = 0.3f;
        _audioSource.PlayScheduled(1 + Random.value);
        Destroy(gameObject, 5f);
    }

    public void KillDefector()
    {
        Kill();
        lock (GameLoop.ScoreLocker)
        {
            GameLoop.Killed++;
        }
    }

    public void CaptureVisitor()
    {
        IsVisitor = false;
        _audioSource.PlayOneShot(SoundManager.Capture);
        ChangeColor(new Color(1, 0.7803586f, 0));
        lock (GameLoop.Locker)
        {
            GameLoop.Workers.Add(gameObject);
        }
        GetComponent<Collider>().enabled = false;
        lock (GameLoop.ScoreLocker)
        {
            GameLoop.Captured++;
        }
        lock (GameLoop.ScreenLocker)
        {
            GameLoop.Stability -= 1f;
            if (GameLoop.Stability < 0) GameLoop.Stability = 0;
        }
    }
    
    public void SetDefector()
    {
        IsDefector = true;
        GetComponent<Collider>().enabled = true;
        ChangeColor(UnityEngine.Color.red, true);
        Wander();
    }

    private void SetVisitor()
    {
        IsVisitor = true;
        GetComponent<Collider>().enabled = true;
        ChangeColor(UnityEngine.Color.green, true);
        Wander();
    }

    private void Wander()
    {
        float x = Random.value * 96 - 48f;
        float z = Random.value * 96 - 48f;
        SendToPosition(new Vector3(x, 0, z));
    }
    
    private void SendToPosition(Vector3 pos)
    {
        _agent.radius = 0.1f;
        _agent.stoppingDistance = 0.2f;
        Arrived = false;
        _agent.destination = pos;
        _animator.SetBool(Worker.IsWalking, true);
    }

    private void ChangeColor(Color c, bool changeMaterial = false)
    {
        if (changeMaterial)
        {
            Material m = Resources.Load("Materials/Worker", typeof(Material)) as Material;
            transform.Find("Mesh1").gameObject.GetComponent<Renderer>().material = Instantiate(m);
        }
        transform.Find("Mesh1").gameObject.GetComponent<Renderer>().material.SetColor(Color, c);
    }

    private void Update()
    {
        if (IsVisitor || IsDefector)
        {
            if (WorkerTimer <= 0)
            {
                if (IsVisitor)
                {
                    lock (GameLoop.ScreenLocker)
                    {
                        GameLoop.Cohesion -= 0.05f;
                        if (GameLoop.Cohesion < 0) GameLoop.Cohesion = 0;
                        GameLoop.MoneyPower += 0.05f;
                        if (GameLoop.MoneyPower > 100) GameLoop.MoneyPower = 100;
                    }
                }
                else if (IsDefector)
                {
                    lock (GameLoop.ScreenLocker)
                    {
                        GameLoop.Stability -= 0.1f;
                        if (GameLoop.Stability < 0) GameLoop.Stability = 0;
                        GameLoop.Cohesion -= 0.1f;
                        if (GameLoop.Cohesion < 0) GameLoop.Cohesion = 0;
                    }
                }
                
                WorkerTimer = 2;
            }
            else
            {
                WorkerTimer -= Time.deltaTime;
            }
        }
        
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance && (!_agent.hasPath || _agent.velocity.sqrMagnitude.Equals(0)))
        {
            _agent.ResetPath();
            Arrived = true;
            _animator.SetBool(IsWalking, false);
            if (IsVisitor || IsDefector)
                Wander();
        }
    }
}