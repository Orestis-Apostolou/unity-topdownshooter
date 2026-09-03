using System.Collections.Generic;
using UnityEngine;

public class PAgentController : AgentController
{
    protected override void Start()
    {
        lastPlayerPos = player.transform.position;
        stateMachine.ChangeState(new PAgentOutOfRangeState(this));
    }

    public static class CoverChecker
    {
        private static List<Collider2D> resultsBuffer = new List<Collider2D>(16);

        public struct CoverResult
        {
            public float totalHP;
            public bool hasLineOfSight;
        }

        public static CoverResult CheckCover(Vector2 origin, Vector2 target, float corridorWidth=0.5f)
        {
            Vector2 toTarget = target - origin;
            float distance = toTarget.magnitude;
            Vector2 center = origin + toTarget * 0.5f;
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

            Vector2 boxSize = new Vector2(distance, corridorWidth);

            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Obstacle"));
            filter.useTriggers = false;

            resultsBuffer.Clear();
            int count = Physics2D.OverlapBox(center, boxSize, angle, filter, resultsBuffer);

            var uniqueObstacles = new HashSet<ObstacleHealth>();
            float totalHP = 0f;

            for (int i = 0; i < count; i++)
            {
                var obstacle = resultsBuffer[i].GetComponentInParent<ObstacleHealth>();
                if (obstacle != null && uniqueObstacles.Add(obstacle))
                {
                    totalHP += obstacle.CurrentHealth();
                }
            }

            return new CoverResult
            {
                totalHP = totalHP,
                hasLineOfSight = count == 0
            };
        }
    }
}
