using System.Collections;
using UnityEngine;

namespace Module.TransformMod
{
    public static class TransformModule
    {
        public static IEnumerator FaceTarget(Transform transform, Vector3 targetPos, float rotateSpeed)
        {
            Vector3 targetDirection;
            Quaternion lookRotation = new Quaternion();
            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.01f)
            {
                targetDirection = (targetPos - transform.position).normalized;
                lookRotation = Quaternion.LookRotation(new Vector3(targetDirection.x, 0f, targetDirection.z));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);

                yield return new WaitForEndOfFrame();
            }
        }
    }
}