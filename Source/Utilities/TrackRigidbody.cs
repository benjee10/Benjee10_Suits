using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WearableProps
{
    public class TrackRigidbody : MonoBehaviour
    {
        public Transform attachTransform;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        private void LateUpdate()
        {
            Vector3 position = attachTransform.TransformPoint(positionOffset);
            Quaternion rotation = attachTransform.rotation * Quaternion.Euler(rotationOffset);

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
