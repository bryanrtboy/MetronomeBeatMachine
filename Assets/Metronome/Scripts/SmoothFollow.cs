using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beats
{
    public class SmoothFollow : MonoBehaviour
    {


        public Transform m_target;
        public float m_zoomOutDistance = 10.0f;
        public float m_zoomInDistance = .5f;
        public float m_height = 5.0f;
        public float m_rotationDamping = .1f;
        public float m_heightDamping = .2f;
        [Tooltip("If true, camera always returns to look at (0,0,0), otherwise it will find Look At Me objects and objects tagged Player")]
        public bool m_useResetTargetToZero = true;

        Vector3 m_newTargetPosition = Vector3.zero;
        float m_newDistanceGoal;
        float m_startDistance;

        GameObject m_currentLookObject;
        LookAtMe m_currentLookAtMe;

        void Awake()
        {
            m_startDistance = m_zoomOutDistance;
            m_newDistanceGoal = m_zoomOutDistance;

        }

        private void Update()
        {
            if (!m_target)
                return;

            if (m_currentLookObject != null)
                m_newTargetPosition = m_currentLookObject.transform.position;

            if (Vector3.Distance(m_target.position, m_newTargetPosition) > .001f)
                m_target.position = Vector3.Lerp(m_target.position, m_newTargetPosition, Time.deltaTime);

            if (!Mathf.Approximately(m_zoomOutDistance, m_newDistanceGoal))
                m_zoomOutDistance = Mathf.Lerp(m_zoomOutDistance, m_newDistanceGoal, Time.deltaTime);

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
            transform.position -= currentRotation * Vector3.forward * m_zoomOutDistance;

            // Set the height of the camera
            transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

            // Always look at the target
            transform.LookAt(m_target);
        }

        public void RemoveTarget()
        {
            if (m_currentLookAtMe)
                m_currentLookAtMe.ReleaseMe();

            m_currentLookAtMe = null;
            m_currentLookObject = null;

            if (m_useResetTargetToZero)
                m_newTargetPosition = Vector3.zero;
            else
                StartCoroutine(FindAndAverageTargets());


            m_newDistanceGoal = m_startDistance;

        }

        public void MakeTarget(GameObject target, LookAtMe lookAtMe)
        {
            if (m_currentLookAtMe)
                m_currentLookAtMe.ReleaseMe();

            m_currentLookObject = target;
            m_newDistanceGoal = m_zoomInDistance;
            m_newTargetPosition = target.transform.position;
            m_currentLookAtMe = lookAtMe;
        }

        IEnumerator FindAndAverageTargets()
        {
            yield return new WaitForEndOfFrame();
            m_newTargetPosition = GetAveragePosition();

        }

        public Vector3 GetAveragePosition()
        {
            Vector3 pos = Vector3.zero;
            List<GameObject> objects = new List<GameObject>();

            LookAtMe[] targetGroup = FindObjectsOfType<LookAtMe>();
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("GameController");

            foreach (LookAtMe l in targetGroup)
                objects.Add(l.gameObject);

            foreach (GameObject g in gameObjects)
                objects.Add(g);

            if (objects.Count < 1)
                return pos;

            foreach (GameObject g in objects)
                pos += g.transform.position;

            pos /= objects.Count;
            return pos;
        }
    }
}