using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    public class SmoothFollow : MonoBehaviour
    {


        public Transform m_target;
        public float m_distance = 10.0f;
        public float m_height = 5.0f;
        public float m_rotationDamping = .1f;
        public float m_heightDamping = .2f;

        Vector3 m_newTargetPosition = Vector3.zero;
        float m_newDistanceGoal;
        float m_startDistance;

        LookAtMe m_currentLookObject;

        void Awake()
        {
            m_startDistance = m_distance;
            m_newDistanceGoal = m_distance;

        }

        private void Update()
        {
            if (!m_target)
                return;

            if (Vector3.Distance(m_target.position, m_newTargetPosition) > .1f)
                m_target.position = Vector3.Lerp(m_target.position, m_newTargetPosition, Time.deltaTime);

            if (!Mathf.Approximately(m_distance, m_newDistanceGoal))
                m_distance = Mathf.Lerp(m_distance, m_newDistanceGoal, Time.deltaTime);

        }


        void LateUpdate()
        {
            if (!m_target)
                return;

            // Calculate the current rotation angles
            var wantedRotationAngle = m_target.eulerAngles.y;
            var wantedHeight = m_target.position.y + m_height;

            var currentRotationAngle = transform.eulerAngles.y;
            var currentHeight = transform.position.y;

            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, m_rotationDamping * Time.deltaTime);

            // Damp the height
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, m_heightDamping * Time.deltaTime);

            // Convert the angle into a rotation
            var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            transform.position = m_target.position;
            transform.position -= currentRotation * Vector3.forward * m_distance;

            // Set the height of the camera
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // Always look at the target
            transform.LookAt(m_target);
        }

        public void RemoveTarget()
        {
            m_currentLookObject = null;
            StartCoroutine(FindAndAverageTargets());

        }

        public void MakeTarget(Vector3 target, LookAtMe lookAtMe)
        {
            if (m_currentLookObject != null)
                m_currentLookObject.ReleaseMe();

            m_currentLookObject = lookAtMe;
            m_newDistanceGoal = 1f;
            m_newTargetPosition = target;
        }

        IEnumerator FindAndAverageTargets()
        {
            m_newDistanceGoal = m_startDistance;
            yield return new WaitForEndOfFrame();
            m_newTargetPosition = GetAveragePosition();

        }

        public Vector3 GetAveragePosition()
        {
            Vector3 pos = Vector3.zero;
            LookAtMe[] targetGroup = FindObjectsOfType<LookAtMe>();

            if (targetGroup == null)
                return pos;

            for (int i = 0; i < targetGroup.Length; i++)
            {
                pos += targetGroup[i].transform.position;
            }

            pos /= targetGroup.Length;
            return pos;
        }
    }
}