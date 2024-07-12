using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : MonoBehaviour
{
    private CropDetails m_cropDetails;
    private int harvestActionCount; //记录对该农作物操作的次数。Record the number of times the crop has been operated on.

    private TileDetails m_tileDetails;

    private Animator m_anim;
    private bool isPlayingAnimation = false;
    private Transform player => FindObjectOfType<Player>().transform;

    public void SetCropDetails(CropDetails cropDetails)
    {
        m_cropDetails = cropDetails;
    }
    public void ProcessToolAction(ItemDetails tool, TileDetails tile)
    {
        m_tileDetails = tile;
        //获得需要对农作物操作的次数，比如树就需要三次，将操作次数绑在树一起（如果真的设计，当然是一种工具对应一类农作物，绝不会这种多对多的设计，而是一对多的设计。一种农作物一定只有一类工具，所以这里不应该是toolID，最好是Category）
        /*Obtain the number of operations required for crops, such as trees requiring three operations. Associate the number of operations with each crop (if actually designed, it would certainly be a one-to-many design where one tool corresponds to one category of crops, not a many-to-many design. Each crop should only have one type of tool, so here it should not be a 'toolID'; 'Category' would be preferable*/
        int operateCount = m_cropDetails.GetOperateCount(tool.itemID);
        if (operateCount == -1) return;

        m_anim = GetComponentInChildren<Animator>();
        if (harvestActionCount < operateCount)
        {
            harvestActionCount++;
            //播放例子效果,audio effects
            if (m_anim != null && m_cropDetails.hasAnimation)
            {
                //获得树这个的子对象，就是树的上半部分，因为我们要使用这个播放摇晃的动画
                // player
                if (player.position.x > transform.position.x)
                {
                    m_anim.SetTrigger("RotateLeft");
                }
                else
                {
                    m_anim.SetTrigger("RotateRight");

                }
                //play particle effect
                if (m_cropDetails.hasParticleEffect)
                {
                    NotifyCenter<SceneEvent, ParticleEffectType, Vector3>.NotifyObservers(SceneEvent.PlayParticleEffect, m_cropDetails.effectType, transform.position + m_cropDetails.effectPos);
                }
                //play audio
            }
        }
        if (harvestActionCount >= operateCount)
        {
            //收获
            if (m_cropDetails.isHeldByPlayer || !m_cropDetails.hasAnimation)
            {
                //生产农作物，如果是农作物就生成在头顶
                SpawnHarvestItems();
            }
            else if (m_cropDetails.hasAnimation && !isPlayingAnimation)
            {
                isPlayingAnimation = true;
                if (player.position.x > transform.position.x)
                {
                    m_anim.SetTrigger("FallLeft");
                }
                else
                {
                    m_anim.SetTrigger("FallRight");
                }
                StartCoroutine(HarvestAfterAnimation());
                //当已经收获完，而对象没有被销毁，还可以被点击就会导致协程多次被调用
            }
        }
    }
    private IEnumerator HarvestAfterAnimation()
    {
        while (!m_anim.GetCurrentAnimatorStateInfo(0).IsName("End"))
        {
            yield return null;
        }
        SpawnHarvestItems();
        //转换成新的物品，比如树变成树桩
        if (m_cropDetails.transformedItemID > 0)
        {
            CreateTransformedCrop();
        }
    }

    private void CreateTransformedCrop()
    {
        m_tileDetails.seedItemID = m_cropDetails.transformedItemID;
        m_tileDetails.daySinceLastHarvest = -1;
        m_tileDetails.growDays = 0;
        NotifyCenter<SceneEvent, bool, bool>.NotifyObservers(SceneEvent.RefreshMap, true);

    }
    private void SpawnHarvestItems()
    {
        for (int i = 0; i < m_cropDetails.producedItemID.Length; i++)
        {
            int amountToProduce = 0;
            if (m_cropDetails.min_producedAccount[i] == m_cropDetails.max_producedAccount[i])
            {
                amountToProduce = m_cropDetails.min_producedAccount[i];
            }
            else
            {
                amountToProduce = Random.Range(m_cropDetails.min_producedAccount[i], m_cropDetails.max_producedAccount[i] + 1);
            }

            if (amountToProduce > 0)
            {
                for (int j = 0; j < amountToProduce; j++)
                {
                    if (m_cropDetails.isHeldByPlayer)
                    {
                        //这个地方就要生成实际的物品，修改角色的动作
                        NotifyCenter<SceneEvent, int, bool>.NotifyObservers(SceneEvent.InstantiateCropAtPlayer, m_cropDetails.producedItemID[i]);
                    }
                    else
                    {
                        var dirX = transform.position.x > player.transform.position.x ? 1 : -1;

                        var spawnPos = new Vector3(transform.position.x + Random.Range(dirX, m_cropDetails.spawnRadius.x * dirX), transform.position.y + Random.Range(-m_cropDetails.spawnRadius.y, m_cropDetails.spawnRadius.y), 0);
                        //生成在世界地图
                        NotifyCenter<SceneEvent, int, Vector3>.NotifyObservers(SceneEvent.CreateItemInScene, m_cropDetails.producedItemID[i], spawnPos);
                    }
                }
            }

        }
        //处理当前农作物的瓦片，是销毁物品还是还原到可以重复收割的状态
        if (m_tileDetails != null)
        {
            m_tileDetails.daySinceLastHarvest++;
            //使用再次生长的天数和再次成长的次数来控制是否可以成长，还有需要多久再次成熟
            if (m_cropDetails.daysToRegrow > 0 && m_tileDetails.daySinceLastHarvest < m_cropDetails.regrowTimes)
            {
                //使用growDays来控制种子的成长状态
                m_tileDetails.growDays = m_cropDetails.TotalGrowthDays - m_cropDetails.daysToRegrow;
                NotifyCenter<SceneEvent, bool, bool>.NotifyObservers(SceneEvent.RefreshMap, true);
            }
            else
            {
                m_tileDetails.daySinceLastHarvest = -1;
                m_tileDetails.seedItemID = -1;
                // m_tileDetails.daySinceDig = -1;
            }
            Destroy(gameObject);
            // Debug.Log(222);
        }
    }
}
