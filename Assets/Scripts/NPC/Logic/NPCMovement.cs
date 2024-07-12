using System;
using System.Collections;
using System.Collections.Generic;
using MFarm.AStar;
using UnityEngine;
using UnityEngine.SceneManagement;

/*这个类就是为了控制NPC移动的类，每个NPC会有一些基础数据来参与计算，比如他所在的场景，出现的位置和他自身的对象，但如果是我的话，或者在大型项目中，一次性加载所有的npc是不可能的，只能动态加载和卸载*/

// [RequireComponent(typeof(Rigidbody2D))]
// [RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NPCMovement : MonoBehaviour
{
    public ScheduleDatalist_SO scheduleData;
    //因为schedule details是继承了IComparable所以这个变量就是将数据库的数据进行排序
    private SortedSet<ScheduleDetails> schedulesSet;
    private ScheduleDetails currentSchedule;
    [SerializeField] private string currentScene;
    private string targetScene;

    private Vector3Int currentGridPos;
    private Vector3Int targetGridPos;
    private Vector3Int nextGridPos;
    private Vector3 nextWorldPos;

    public string StartScene { set => currentScene = value; }

    [Header("移动属性")]
    public float normalSpeed = 2f;
    private float minSpeed = 1f;
    private float maxSpeed = 3f;

    private Vector2 dir;
    public bool isMoving;
    public bool moveNpc;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private BoxCollider2D coll;
    private Stack<MovementStep> movementSteps;

    private Grid grid;

    private bool isInitialized = false;

    private bool isSceneLoaded;

    private float animationBreakTime;
    private bool canPlayStopAnimation;

    private AnimationClip stopAnimationClip;
    public AnimationClip blanksAnimationClip;
    private AnimatorOverrideController animatorOverride;
    private TimeSpan GameTime => TimeManager.Instance.GameTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        movementSteps = new Stack<MovementStep>();
        //通过anim生成一个子类对象，然后将实际需要的动画控制器设置为anim的运行时需要的控制器，达到切换控制器的目的，因为切换控制器就意味着切换动画
        animatorOverride = new AnimatorOverrideController(anim.runtimeAnimatorController);
        anim.runtimeAnimatorController = animatorOverride;
    }
    private void FixedUpdate()
    {
        if (isSceneLoaded)
            Movement();
    }

    private void Update()
    {
        if (isSceneLoaded)
        {
            SwitchAnimation();
        }
        animationBreakTime -= Time.deltaTime;
        canPlayStopAnimation = animationBreakTime <= 0;
    }
    private void CheckVisible()
    {
        if (currentScene == SceneManager.GetActiveScene().name)
        {
            SetActiveInScene(true);
        }
        else
        {
            SetActiveInScene(false);
        }
    }
    private void SetActiveInScene(bool isActive)
    {
        spriteRenderer.enabled = isActive;
        coll.enabled = isActive;
        // transform.GetChild(0).gameObject.SetActive(isActive);
    }



    private void OnEnable()
    {
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter += OnSceneChange;
    }
    private void OnDisable()
    {
        NotifyCenter<SceneEvent, bool, bool>.notifyCenter -= OnSceneChange;

    }

    private void OnSceneChange(SceneEvent sceneEvent, bool arg2, bool arg3)
    {
        if (sceneEvent == SceneEvent.AfterLoadScene)
        {
            grid = FindObjectOfType<Grid>();
            CheckVisible();

            if (!isInitialized)
            {
                InitNPC();
                isInitialized = true;
            }
            isSceneLoaded = true;
        }
        else if (sceneEvent == SceneEvent.BeforeLoadScene)
        {
            isSceneLoaded = false;
        }
    }

    private void InitNPC()
    {
        targetScene = currentScene;

        //让人物处于网格的中心
        currentGridPos = grid.WorldToCell(transform.position);

        transform.position = new Vector3(currentGridPos.x + Settings.gridCellSize / 2f, currentGridPos.y + Settings.gridCellSize / 2f, 0);

        targetGridPos = currentGridPos;
    }

    private void Movement()
    {
        if (!moveNpc)
        {
            if (movementSteps.Count > 0)
            {
                MovementStep step = movementSteps.Pop();
                currentScene = step.sceneName;
                //应该时时检测当前场景是否是npc应该在的场景，因为人物player会一直移动，甚至可能切换场景，所以一旦场景发生了切换就要改变npc的显示状态
                CheckVisible();
                nextGridPos = (Vector3Int)step.gridCoordinate;
                // Debug.Log($"show step {step.hour}{step.minute}{step.second} {GameTime}");
                //拿到这个格子的时间戳，如果当前的时间没有到下一个格子的时间戳就缓慢走过去，否则就瞬移过去
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);
                MoveToGridPosition(nextGridPos, stepTime);
            }
            else if (!isMoving && canPlayStopAnimation)
            {
                StartCoroutine(SetStopAnimation());
            }
        }

    }

    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
    {
        StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        moveNpc = true;
        nextWorldPos = GetWorldPosition(gridPos);
        // Debug.Log($"stepTime and GameTime {stepTime}and{GameTime}");
        if (stepTime > GameTime)
        {
            //得到可以用来移动的时间
            float timeForMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            float distance = Vector3.Distance(transform.position, nextWorldPos);
            float speed = Mathf.Max(minSpeed, distance / timeForMove / Settings.secondThreshold);

            if (speed <= maxSpeed)
            {
                //Settings.pixelSiz=0.05是因为这个项目每个格子的大小是20*20的，也就说一个格子包含了20个像素宽，0.05代表一个像素
                while (Vector3.Distance(transform.position, nextWorldPos) > Settings.pixelSize)
                {
                    // Debug.Log(1232312);
                    dir = (nextWorldPos - transform.position).normalized;
                    Vector2 posOffset = new Vector2(dir.x * speed * Time.fixedDeltaTime, dir.y * speed * Time.fixedDeltaTime);
                    rb.MovePosition(rb.position + posOffset);
                    yield return new WaitForFixedUpdate();

                }
            }
        }
        rb.position = nextWorldPos;
        currentGridPos = gridPos;
        nextGridPos = currentGridPos;
        moveNpc = false;
    }

    private Vector3 GetWorldPosition(Vector3Int gridPos)
    {
        Vector3 worldPos = grid.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2f, 0);
    }
    public void BuildPath(ScheduleDetails schedule)
    {
        movementSteps.Clear();
        currentSchedule = schedule;
        targetGridPos = (Vector3Int)schedule.targetGridPosition;
        stopAnimationClip = schedule.clipAnimationStop;
        //通过A*算法得到了路径的堆栈
        if (schedule.targetSceneName == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetSceneName, (Vector2Int)currentGridPos, schedule.targetGridPosition, movementSteps);
        }

        if (movementSteps.Count > 1)
        {
            //更新每一步的时间戳，如果堆栈是存在的，那么就把堆栈中的每一步都赋值时间戳
            UpdateTimeOnPath();
        }

    }

    private void UpdateTimeOnPath()
    {
        MovementStep previousStep = null;
        TimeSpan currentGameTime = GameTime;
        //movementSteps这个包含从起点到终点的路径因为起点和终点都被放入了closelist
        foreach (MovementStep step in movementSteps)
        {
            if (previousStep == null)
            {
                previousStep = step;
            }
            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            //每走过一个格子所需要的时间
            TimeSpan eachGridStepTime;

            if (MoveInDiagonal(step, previousStep))
            {
                eachGridStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.secondThreshold));
            }
            else
            {
                eachGridStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.secondThreshold));
            }
            currentGameTime = currentGameTime.Add(eachGridStepTime);
            previousStep = step;
        }
    }
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }

    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPosition(targetGridPos);
        anim.SetBool("IsMoving", isMoving);
        if (isMoving)
        {
            anim.SetBool("Exit", true);
            // 设定一个阈值，避免动画频繁切换
            float threshold = 0.1f;

            // 如果y方向的输入大于x方向的输入，且大于设定的阈值，则优先播放y方向动画
            if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x) + threshold)
            {
                anim.SetFloat("DirX", 0);
                anim.SetFloat("DirY", dir.y);
            }
            // 如果x方向的输入大于y方向的输入，且大于设定的阈值，则优先播放x方向动画
            else if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y) + threshold)
            {
                anim.SetFloat("DirX", dir.x);
                anim.SetFloat("DirY", 0);
            }
            // 如果两个方向的输入差别不大，则不切换动画
            else
            {
                anim.SetFloat("DirX", anim.GetFloat("DirX"));
                anim.SetFloat("DirY", anim.GetFloat("DirY"));
            }
        }
        else
        {
            anim.SetBool("Exit", false);

        }
    }

    private IEnumerator SetStopAnimation()
    {
        anim.SetFloat("DirX", 0);
        anim.SetFloat("DirY", -1);

        animationBreakTime = Settings.animationBreakTime;
        if (stopAnimationClip != null)
        {
            animatorOverride[blanksAnimationClip] = stopAnimationClip;
            anim.SetBool("EventAnimation", true);
            yield return null;
            anim.SetBool("EventAnimation", false);

        }
        else
        {
            animatorOverride[stopAnimationClip] = blanksAnimationClip;
            anim.SetBool("EventAnimation", false);

        }
    }
}
