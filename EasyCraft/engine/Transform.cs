using SharpDX.Windows;
using System;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Mathematics.Interop;
using SharpDX;
using System.Collections.Generic;
using EasyCraft.engine.extensions;

namespace EasyCraft.engine
{
    public enum Space
    {
        Object,
        World
    }

    public class Transform
    {
        /// <summary>
        /// The world position of the transform
        /// </summary>
        public Vector3 position { get { if (parent != null) return parent.position + localPosition; return localPosition; } set { if (parent != null) localPosition = value - parent.position; else localPosition = value; } }

        /// <summary>
        /// The local position of the transform
        /// </summary>
        public Vector3 localPosition { get; set; }

        /// <summary>
        /// The world rotation of the transform
        /// </summary>
        public Quaternion rotation { get { if (parent != null) return parent.rotation * localRotation; return localRotation; } set { if (parent != null) { Quaternion p = parent.rotation;p.Invert(); localRotation = p * value; } else localRotation = value; } }

        /// <summary>
        /// The local rotation of the transform
        /// </summary>
        public Quaternion localRotation { get; set; }

        /// <summary>
        /// The local scale of the transform
        /// </summary>
        public Vector3 localScale { get; set; }

        /// <summary>
        /// The lossy (global) scale of the transform
        /// </summary>
        public Vector3 lossyScale { get { if (parent != null) return parent.lossyScale * localScale; return localScale; } }

        public Vector3 right { get { return Vector3.Transform(Vector3.Right, rotation); } }
        public Vector3 up { get { return Vector3.Transform(Vector3.Up, rotation); } }
        public Vector3 forward { get { return Vector3.Transform(Vector3.ForwardLH, rotation); } }

        public Transform root { get { if (parent != null) return parent.root; return this; } }
        public Transform parent { get; private set; }
        public int childCount { get => children.Count; }

        public Matrix WorldMatrix
        {
            get
            {
                return Matrix.Scaling(localScale) * Matrix.RotationQuaternion(rotation) * Matrix.Translation(position);
            }
        }

        private List<Transform> children = new List<Transform>();

        public Transform()
        {
            position = new Vector3(0, 0, 0);
            rotation = Quaternion.Identity;
            localScale = new Vector3(1, 1, 1);
        }

        public void Translate(Vector3 amount)
        {
            Translate(amount, Space.Object);
        }

        public void Translate(Vector3 amount, Space space)
        {
            if (space == Space.World)
                position += amount;
            else if (space == Space.Object)
                localPosition += amount;
        }

        public void Rotate(Vector3 eulers)
        {
            Rotate(eulers, Space.Object);
        }

        public void Rotate(Vector3 eulers, Space space)
        {
            if(space == Space.Object)
                localRotation *= Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(eulers.Y), MathUtil.DegreesToRadians(eulers.X), MathUtil.DegreesToRadians(eulers.Z));
            else if(space == Space.World)
                rotation *= Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(eulers.Y), MathUtil.DegreesToRadians(eulers.X), MathUtil.DegreesToRadians(eulers.Z));
        }

        public void SetParent(Transform parent)
        {
            this.parent = parent;
            this.parent.children.Add(this);
        }

        public Transform GetChild(int index)
        {
            return children[index];
        }
    }
}
