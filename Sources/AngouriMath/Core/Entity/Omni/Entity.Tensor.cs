﻿/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;
using System.Linq;
using GenericTensor.Core;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    using GenTensor = GenTensor<Entity, Entity.Tensor.EntityTensorWrapperOperations>;
    partial record Entity
    {
        #region Tensor

        /// <summary>Basic tensor implementation: <a href="https://en.wikipedia.org/wiki/Tensor"/></summary>
#pragma warning disable CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters
        public sealed partial record Tensor(GenTensor InnerTensor) : Entity
#pragma warning restore CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Tensor New(GenTensor innerTensor) =>
                innerTensor.Iterate().All(tup => ReferenceEquals(InnerTensor.GetValueNoCheck(tup.Index), tup.Value))
                ? this
                : new Tensor(innerTensor);
            internal override Priority Priority => Priority.Leaf;
            internal Tensor Elementwise(Func<Entity, Entity> operation) =>
                New(GenTensor.CreateTensor(InnerTensor.Shape, indices => operation(InnerTensor.GetValueNoCheck(indices))));
            internal Tensor Elementwise(Tensor other, Func<Entity, Entity, Entity> operation) =>
                Shape != other.Shape
                ? throw new InvalidShapeException("Arguments should be of the same shape to apply elementwise operation")
                : New(GenTensor.CreateTensor(InnerTensor.Shape, indices =>
                        operation(InnerTensor.GetValueNoCheck(indices), other.InnerTensor.GetValueNoCheck(indices))));
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => Elementwise(element => element.Replace(func));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => InnerTensor.Iterate().Select(tup => tup.Value).ToArray();

#pragma warning disable CS1591
            public readonly struct EntityTensorWrapperOperations : IOperations<Entity>
            {
                public Entity Add(Entity a, Entity b) => a + b;
                public Entity Subtract(Entity a, Entity b) => a - b;
                public Entity Multiply(Entity a, Entity b) => a * b;
                public Entity Negate(Entity a) => -a;
                public Entity Divide(Entity a, Entity b) => a / b;
                public Entity CreateOne() => Number.Integer.One;
                public Entity CreateZero() => Number.Integer.Zero;
                public Entity Copy(Entity a) => a;
#pragma warning disable CA1822 // Mark members as static
                public Entity Forward(Entity a) => a;
#pragma warning restore CA1822 // Mark members as static
                public bool AreEqual(Entity a, Entity b) => a == b;
                public bool IsZero(Entity a) => a == 0;
                public string ToString(Entity a) => a.Stringize();

                public byte[] Serialize(Entity a)
                {
                    throw FutureReleaseException.Raised("Serialization");
                }

                public Entity Deserialize(byte[] data)
                {
                    throw FutureReleaseException.Raised("Deserialization");
                }
            }
#pragma warning restore CS1591
            /// <summary>List of <see cref="int"/>s that stand for dimensions</summary>
            public TensorShape Shape => InnerTensor.Shape;

            /// <summary>Number of dimensions: 2 for matrix, 1 for vector</summary>
            public int Dimensions => Shape.Count;

            /// <summary>
            /// List of dimensions
            /// If you need matrix, list 2 dimensions 
            /// If you need vector, list 1 dimension (length of the vector)
            /// You can't list 0 dimensions
            /// </summary>
            public Tensor(Func<int[], Entity> operation, params int[] dims) : this(GenTensor.CreateTensor(new(dims), operation)) { }

            /// <summary>
            /// Access the tensor if it is a vector
            /// </summary>
            public Entity this[int i] => InnerTensor[i];

            /// <summary>
            /// Access the tensor if it is a matrix
            /// </summary>
            public Entity this[int x, int y] => InnerTensor[x, y];

            /// <summary>
            /// Access the tensor if it is a 3D tensor
            /// </summary>
            public Entity this[int x, int y, int z] => InnerTensor[x, y, z];

            /// <summary>
            /// Access the tensor if it is a tensor of greater dimension than 3
            /// </summary>
            public Entity this[params int[] dims] => InnerTensor[dims];

            /// <summary>
            /// Checks whether the tensor is one-dimensional,
            /// that is, vector
            /// </summary>
            public bool IsVector => InnerTensor.IsVector;

            /// <summary>
            /// Checks whether the tensor is two-dimensional,
            /// that is, matrix
            /// </summary>
            public bool IsMatrix => InnerTensor.IsMatrix;

            /// <summary>Changes the order of axes</summary>
            public void Transpose(int a, int b) => InnerTensor.Transpose(a, b);

            /// <summary>Changes the order of axes in matrix</summary>
            public void Transpose()
            {
                if (IsMatrix) InnerTensor.TransposeMatrix();
                else throw new InvalidMatrixOperationException("Specify axes numbers for non-matrices");
            }

            // We do not need to use Gaussian elimination here
            // since we anyway get N! memory use
            /// <summary>
            /// Finds the symbolical determinant via Laplace's method
            /// </summary>
            public Entity Determinant() => InnerTensor.DeterminantLaplace().InnerSimplified;

            /// <summary>Inverts all matrices in a tensor</summary>
            public Tensor Inverse()
            {
                var cp = InnerTensor.Copy(false);
                cp.TensorMatrixInvert();
                return new Tensor(cp);
            }
        }
        #endregion
    }
}
