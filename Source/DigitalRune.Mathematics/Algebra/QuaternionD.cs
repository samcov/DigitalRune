// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
#if !NETFX_CORE && !PORTABLE
using DigitalRune.Mathematics.Algebra.Design;
#endif
#if XNA || MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Defines a quaternion (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A quaternion consists of a scalar component <i>w</i> and a vector component 
  /// <i><b>v</b> = (x, y, z)</i>. Alternatively it can be represented as a complex number with 
  /// three imaginary parts <i>w + <b>i</b>x + <b>j</b>y + <b>k</b>z</i>, or as a 4-dimensional 
  /// vector <i>(w, x, y, z)</i>
  /// </para>
  /// <para>
  /// Due to common notation, the quaternion components are stored in the order:
  /// <i>(w, x, y, z)</i>.
  /// </para>
  /// <para>
  /// <b>Unit Quaternions:</b> 
  /// <para>
  /// A <i>unit quaternion</i> is a quaternion <i>q</i> where N(<i>q</i>) = 1. (See 
  /// <see cref="Norm"/>.) A unit quaternion can be represented by 
  /// </para>
  /// <para>
  /// <i>q</i> = cos<i>θ</i> + <i><b>u</b></i>sin<i>θ</i>,
  /// </para>
  /// <para>
  /// where <i><b>u</b></i> as a 3D vector has a length of 1. By applying Euler's identity for 
  /// complex numbers the quaternion can be written in exponential notation: 
  /// </para>
  /// <para>
  /// <i>q</i> = e<sup><i><b>u</b></i><i>θ</i></sup> = cos<i>θ</i> + <i><b>u</b></i>sin<i>θ</i>
  /// </para>
  /// </para>
  /// <para>
  /// Several methods, such as <see cref="Ln(QuaternionD)"/>, require that the quaternion is a unit 
  /// quaternion.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
#if !NETFX_CORE && !PORTABLE
  [TypeConverter(typeof(QuaternionDConverter))]
#endif
#if !XBOX && !UNITY
  [DataContract]
#endif
  public struct QuaternionD : IEquatable<QuaternionD>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a <see cref="QuaternionD"/> with all of its components set to zero.
    /// </summary>
    public static readonly QuaternionD Zero = new QuaternionD(0, 0, 0, 0);

    /// <summary>
    /// Returns the identity <see cref="QuaternionD"/> (1, 0, 0, 0).
    /// </summary>
    /// <remarks>
    /// The identity quaternion is a unit quaternion that specifies no rotation.
    /// </remarks>
    public static readonly QuaternionD Identity = new QuaternionD(1, 0, 0, 0);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The w component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double W;

    /// <summary>
    /// The x component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double X;

    /// <summary>
    /// The y component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double Y;

    /// <summary>
    /// The z component.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !XBOX && !UNITY
    [DataMember]
#endif
    public double Z;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the angle of the rotation around <see cref="Axis"/>.
    /// </summary>
    /// <value>The angle in radians.</value>
    /// <remarks>
    /// <para>
    /// Setting the angle influences all components of the quaternion. The result is a unit
    /// quaternion that specifies a rotation of <i>angle</i> radians around the axis given by 
    /// <see cref="Axis"/>.
    /// </para>
    /// <para>
    /// This property assumes that the quaternion is a unit quaternion. It returns
    /// <see cref="Double.NaN"/> if the w component is numerically greater than 1.0 or less than
    /// -1.0.
    /// </para>
    /// </remarks>
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public double Angle
    {
      get
      {
        double w = W;

        // Return NaN is if w is not in [-1, 1] (with numerical tolerance).
        if (Numeric.IsGreater(Math.Abs(w), 1))
          return double.NaN;

        // Clamp to allowed range.
        w = MathHelper.Clamp(w, -1, 1);

        return Math.Acos(w) * 2;
      }
      set { this = CreateRotation(Axis, value); }
    }


    /// <summary>
    /// Gets or sets the vector part (x, y, z).
    /// </summary>
    /// <value>The vector part (x, y, z).</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public Vector3D V
    {
      get { return new Vector3D(X, Y, Z); }
      set
      {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
      }
    }


    /// <summary>
    /// Gets or sets the normalized unit vector with the direction of the rotation axis.
    /// </summary>
    /// <value>
    /// The normalized unit vector with the direction of the rotation axis.
    /// </value>
    /// <remarks>
    /// <para>
    /// Setting the axis influences all components of the quaternion. The result is a unit
    /// quaternion that specifies a rotation of <see cref="Angle"/> radians around the specified
    /// axis.
    /// </para>
    /// <para>
    /// If the quaternion represents "no rotation" (rotation angle is 0), the axis vector is 
    /// (0, 0, 0).
    /// </para>
    /// </remarks>
    [XmlIgnore]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public Vector3D Axis
    {
      get
      {
        var v = new Vector3D(X, Y, Z);

        // If we can normalize v, this is our axis.
        if (v.TryNormalize())
          return v;

        // Could not normalize v. --> No rotation, no axis.
        return Vector3D.Zero;
      }
      set
      {
        if (!value.IsNumericallyZero)
          this = CreateRotation(value, Angle);
        else
          this = Identity;
      }
    }


    /// <summary>
    /// Returns the conjugate of the quaternion.
    /// </summary>
    /// <value>The conjugate of this quaternion.</value>
    /// <remarks>
    /// <para>
    /// The conjugate of a quaternion is calculated by negating the vector component.
    /// </para>
    /// <para>
    /// <i>q<sup>*</sup> = w - <b>i</b>x - <b>j</b>y - <b>k</b>z</i>
    /// </para>
    /// <para>
    /// The property does not change this quaternion. To conjugate this instance you need to call 
    /// <see cref="Conjugate"/>.
    /// </para>
    /// </remarks>
    public QuaternionD Conjugated
    {
      get
      {
        QuaternionD result = this;
        result.Conjugate();
        return result;
      }
    }


    /// <summary>
    /// Returns the inverse of this quaternion.
    /// </summary>
    /// <value>The inverse of this quaternion.</value>
    /// <remarks>
    /// <para>
    /// The (multiplicative) inverse of a quaternion is calculated by using the following formula:
    /// </para>
    /// <para>
    /// <i>q<sup>-1</sup> = q<sup>*</sup> / (q q<sup>*</sup>) = q<sup>*</sup> / </i>N(<i>q</i>)
    /// </para>
    /// <para>
    /// The property does not change this quaternion. To invert this instance you need to call 
    /// <see cref="Invert"/>.
    /// </para>
    /// <para>
    /// The inverse of a unit quaternion is the same as its conjugate. You might consider using the
    /// property <see cref="Conjugated"/> because it is faster than <see cref="Inverse"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The length of the quaternion is zero. The quaternion cannot be inverted.
    /// </exception>
    public QuaternionD Inverse
    {
      get
      {
        QuaternionD result = this;
        result.Invert();
        return result;
      }
    }


    /// <summary>
    /// Returns the normalized quaternion.
    /// </summary>
    /// <value>The normalized quaternion.</value>
    /// <remarks>
    /// The property does not change this instance. To normalize this instance you need to call 
    /// <see cref="Normalize"/>.
    /// </remarks>
    /// <exception cref="DivideByZeroException">
    /// The length of the quaternion is zero. The quaternion cannot be normalized.
    /// </exception>
    public QuaternionD Normalized
    {
      get
      {
        QuaternionD result = this;
        result.Normalize();
        return result;
      }
    }


    /// <summary>
    /// Returns the modulus (length).
    /// </summary>
    /// <value>The modulus (length).</value>
    /// <remarks>
    /// <para>
    /// The <i>modulus</i> is also known as the <i>magnitude</i> or simply the <i>length</i> of a
    /// quaternion. It is calculated with the following formula:
    /// </para>
    /// <para>
    /// || <i>q</i> || = Sqrt(<i>w<sup>2</sup> + x<sup>2</sup> + y<sup>2</sup> + z<sup>2</sup></i>)
    /// </para>
    /// </remarks>
    public double Modulus
    {
      get { return Math.Sqrt(W * W + X * X + Y * Y + Z * Z); }
    }


    /// <summary>
    /// Returns the norm (<i>length<sup>2</sup></i>).
    /// </summary>
    /// <value>The norm.</value>
    /// <remarks>
    /// <para>
    /// The norm of a quaternion is calculated with the following formula: 
    /// </para>
    /// <para>
    /// N(<i>q</i>) = <i>w<sup>2</sup> + x<sup>2</sup> + y<sup>2</sup> + z<sup>2</sup></i>
    /// </para>
    /// </remarks>
    public double Norm
    {
      get { return W * W + X * X + Y * Y + Z * Z; }
    }


    /// <summary>
    /// Gets a value indicating whether a component of the quaternion is <see cref="double.NaN"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a component of the quaternion is <see cref="double.NaN"/>; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool IsNaN
    {
      get { return Numeric.IsNaN(W) || Numeric.IsNaN(X) || Numeric.IsNaN(Y) || Numeric.IsNaN(Z); }
    }


    /// <summary>
    /// Returns a value indicating whether this quaternion is normalized (the <see cref="Modulus"/> 
    /// is numerically equal to 1).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="QuaternionD"/> is normalized; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <see cref="IsNumericallyNormalized"/> compares the <see cref="Modulus"/> (length) of this 
    /// quaternion against 1.0 using the default tolerance value (see 
    /// <see cref="Numeric.EpsilonD"/>).
    /// </remarks>
    public bool IsNumericallyNormalized
    {
      get
      {
        // We compare the squared length (Norm) with 1.0.
        return Numeric.AreEqual(Norm, 1.0);
      }
    }


    /// <summary>
    /// Gets or sets the component at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>The component at <paramref name="index"/>.</value>
    /// <remarks>
    /// The index is zero based: w = quaternion[0], x = quaternion[1], ... z = quaternion[3]. 
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="index"/> is out of range.
    /// </exception>
    public double this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return W;
          case 1: return X;
          case 2: return Y;
          case 3: return Z;
          default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
        }
      }
      set
      {
        switch (index)
        {
          case 0: W = value; break;
          case 1: X = value; break;
          case 2: Y = value; break;
          case 3: Z = value; break;
          default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> class.
    /// </summary>
    /// <param name="w">The initial value for the w component.</param>
    /// <param name="x">The initial value for the x component.</param>
    /// <param name="y">The initial value for the y component.</param>
    /// <param name="z">The initial value for the z component.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public QuaternionD(double w, double x, double y, double z)
    {
      W = w;
      X = x;
      Y = y;
      Z = z;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> class.
    /// </summary>
    /// <param name="components">
    /// Array with the initial values for the components w, x, y and z.
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="components"/> has less than 4 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="components"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public QuaternionD(double[] components)
    {
      W = components[0];
      X = components[1];
      Y = components[2];
      Z = components[3];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> class.
    /// </summary>
    /// <param name="components">
    /// List with the initial values for the components w, x, y and z.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="components"/> has less than 4 elements.
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// <paramref name="components"/> must not be <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public QuaternionD(IList<double> components)
    {
      W = components[0];
      X = components[1];
      Y = components[2];
      Z = components[3];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> class.
    /// </summary>
    /// <param name="w">The initial value for scalar component w.</param>
    /// <param name="v">The initial values for the vector component (x, y, z).</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public QuaternionD(double w, Vector3D v)
    {
      W = w;
      X = v.X;
      Y = v.Y;
      Z = v.Z;
    }
    #endregion


    //--------------------------------------------------------------
    #region Interfaces and Overrides
    //--------------------------------------------------------------

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = W.GetHashCode();
        hashCode = (hashCode * 397) ^ X.GetHashCode();
        hashCode = (hashCode * 397) ^ Y.GetHashCode();
        hashCode = (hashCode * 397) ^ Z.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <overloads>
    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and 
    /// represent the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is QuaternionD && this == (QuaternionD)obj;
    }


    #region IEquatable<QuaternionD> Members
    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(QuaternionD other)
    {
      return this == other;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Returns the string representation of this quaternion.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns the string representation of this quaternion.
    /// </summary>
    /// <returns>The string representation of this quaternion.</returns>
    public override string ToString()
    {
      return ToString(CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Returns the string representation of this vector using the specified culture-specific format
    /// information.
    /// </summary>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
    /// </param>
    /// <returns>The string representation of this quaternion.</returns>
    public string ToString(IFormatProvider provider)
    {
      return string.Format(provider,
                           "({0}; ({1}; {2}; {3}))",
                           W, X, Y, Z);
    }
    #endregion


    //--------------------------------------------------------------
    #region Overloaded Operators
    //--------------------------------------------------------------

    /// <summary>
    /// Negates a quaternion.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <returns>The negated quaternion.</returns>
    public static QuaternionD operator -(QuaternionD quaternion)
    {
      quaternion.X = -quaternion.X;
      quaternion.Y = -quaternion.Y;
      quaternion.Z = -quaternion.Z;
      quaternion.W = -quaternion.W;
      return quaternion;
    }


    /// <summary>
    /// Negates a quaternion.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <returns>The negated quaternion.</returns>
    public static QuaternionD Negate(QuaternionD quaternion)
    {
      quaternion.X = -quaternion.X;
      quaternion.Y = -quaternion.Y;
      quaternion.Z = -quaternion.Z;
      quaternion.W = -quaternion.W;
      return quaternion;
    }


    /// <summary>
    /// Adds two quaternions.
    /// </summary>
    /// <param name="quaternion1">The first quaternion.</param>
    /// <param name="quaternion2">The second quaternion.</param>
    /// <returns>The sum of the two quaternions.</returns>
    public static QuaternionD operator +(QuaternionD quaternion1, QuaternionD quaternion2)
    {
      quaternion1.X += quaternion2.X;
      quaternion1.Y += quaternion2.Y;
      quaternion1.Z += quaternion2.Z;
      quaternion1.W += quaternion2.W;
      return quaternion1;
    }


    /// <summary>
    /// Adds two quaternions.
    /// </summary>
    /// <param name="quaternion1">The first quaternion.</param>
    /// <param name="quaternion2">The second quaternion.</param>
    /// <returns>The sum of the two quaternions.</returns>
    public static QuaternionD Add(QuaternionD quaternion1, QuaternionD quaternion2)
    {
      quaternion1.X += quaternion2.X;
      quaternion1.Y += quaternion2.Y;
      quaternion1.Z += quaternion2.Z;
      quaternion1.W += quaternion2.W;
      return quaternion1;
    }


    /// <summary>
    /// Subtracts a quaternion from a quaternion.
    /// </summary>
    /// <param name="minuend">The first quaternion (minuend).</param>
    /// <param name="subtrahend">The second quaternion (subtrahend).</param>
    /// <returns>The difference of the two quaternions.</returns>
    public static QuaternionD operator -(QuaternionD minuend, QuaternionD subtrahend)
    {
      minuend.X -= subtrahend.X;
      minuend.Y -= subtrahend.Y;
      minuend.Z -= subtrahend.Z;
      minuend.W -= subtrahend.W;
      return minuend;
    }


    /// <summary>
    /// Subtracts a quaternion from a quaternion.
    /// </summary>
    /// <param name="minuend">The first quaternion (minuend).</param>
    /// <param name="subtrahend">The second quaternion (subtrahend).</param>
    /// <returns>The difference of the two quaternions.</returns>
    public static QuaternionD Subtract(QuaternionD minuend, QuaternionD subtrahend)
    {
      minuend.X -= subtrahend.X;
      minuend.Y -= subtrahend.Y;
      minuend.Z -= subtrahend.Z;
      minuend.W -= subtrahend.W;
      return minuend;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a quaternion by a scalar or a quaternion.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a quaternion by a scalar.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>
    /// The quaternion with each component multiplied by <paramref name="scalar"/>.
    /// </returns>
    public static QuaternionD operator *(QuaternionD quaternion, double scalar)
    {
      quaternion.W *= scalar;
      quaternion.X *= scalar;
      quaternion.Y *= scalar;
      quaternion.Z *= scalar;
      return quaternion;
    }


    /// <summary>
    /// Multiplies a quaternion by a scalar.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>
    /// The quaternion with each component multiplied by <paramref name="scalar"/>.
    /// </returns>
    public static QuaternionD operator *(double scalar, QuaternionD quaternion)
    {
      quaternion.W *= scalar;
      quaternion.X *= scalar;
      quaternion.Y *= scalar;
      quaternion.Z *= scalar;
      return quaternion;
    }


    /// <overloads>
    /// <summary>
    /// Multiplies a quaternion by a scalar or a quaternion.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Multiplies a quaternion by a scalar.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>
    /// The quaternion with each component multiplied by <paramref name="scalar"/>.
    /// </returns>
    public static QuaternionD Multiply(double scalar, QuaternionD quaternion)
    {
      quaternion.W *= scalar;
      quaternion.X *= scalar;
      quaternion.Y *= scalar;
      quaternion.Z *= scalar;
      return quaternion;
    }


    /// <summary>
    /// Multiplies two quaternions.
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <returns>The product of the two quaternions.</returns>
    /// <remarks>
    /// <para>
    /// If the quaternions are unit quaternions, then each quaternion represents a rotation in
    /// 3-dimensional space. The product of two unit quaternions is the concatenation of the two
    /// rotations.
    /// </para>
    /// <para>
    /// <i>q<sub>2</sub> q<sub>1</sub></i> is the same as <i>M<sub>2</sub> . M<sub>1</sub></i>,
    /// where <i>M<sub>2</sub></i> and <i>M<sub>1</sub></i> are the equivalent matrices 
    /// (<i>M<sub>1</sub></i> specifies the same rotation as <i>q<sub>1</sub></i> and 
    /// <i>M<sub>2</sub></i> specifies the same rotation as <i>q<sub>2</sub></i>).
    /// </para>
    /// <para>
    /// The multiplication is non-commutative. The operation is also known as the <i>Grassman
    /// product</i> of quaternions.
    /// </para>
    /// <para>
    /// The multiplication is defined as:
    /// </para>
    /// <para>
    /// <i>q<sub>1</sub></i> <i>q<sub>2</sub></i> = (<i>w<sub>1</sub></i> <i>w<sub>2</sub></i> - 
    /// <i><b>v</b><sub>1</sub></i> ∙ <i><b>v</b><sub>2</sub></i>, 
    /// <i><b>v</b><sub>1</sub></i> x <i><b>v</b><sub>2</sub></i> + <i>w<sub>1</sub></i> 
    /// <i><b>v</b><sub>2</sub></i> + <i>w<sub>2</sub></i> <i><b>v</b><sub>1</sub></i>)
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static QuaternionD operator *(QuaternionD q1, QuaternionD q2)
    {
      QuaternionD product;
      product.W = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
      product.X = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
      product.Y = q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X;
      product.Z = q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W;
      return product;

      // An alternative way to write the product:
      // product.W = q1.W * q2.W - Vector3D.Dot(q1.V, q2.V);
      // product.V = Vector3D.Cross(q1.V, q2.V) + q1.W * q2.V + q2.W * q1.V;
    }


    /// <summary>
    /// Multiplies two quaternions.
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <returns>The product of the two quaternions.</returns>
    /// <remarks>
    /// <para>
    /// If the quaternions are unit quaternions, then each quaternion represents a rotation in
    /// 3-dimensional space. The product of two unit quaternions is the concatenation of the two
    /// rotations.
    /// </para>
    /// <para>
    /// <i>q<sub>1</sub> . q<sub>2</sub></i> is the same as <i>M<sub>1</sub> . M<sub>2</sub></i>,
    /// where <i>M<sub>1</sub></i> and <i>M<sub>2</sub></i> are the equivalent matrices 
    /// (<i>M<sub>1</sub></i> specifies the same rotation as <i>q<sub>1</sub></i> and 
    /// <i>M<sub>2</sub></i> specifies the same rotation as <i>q<sub>2</sub></i>).
    /// </para>
    /// <para>
    /// The multiplication is non-commutative. The operation is also known as the <i>Grassman
    /// product</i> of quaternions.
    /// </para>
    /// <para>
    /// The multiplication is defined as:
    /// </para>
    /// <para>
    /// <i>q<sub>1</sub></i> <i>q<sub>2</sub></i> = (<i>w<sub>1</sub></i> <i>w<sub>2</sub></i> - 
    /// <i><b>v</b><sub>1</sub></i> ∙ <i><b>v</b><sub>2</sub></i>, 
    /// <i><b>v</b><sub>1</sub></i> x <i><b>v</b><sub>2</sub></i> + <i>w<sub>1</sub></i> 
    /// <i><b>v</b><sub>2</sub></i> + <i>w<sub>2</sub></i> <i><b>v</b><sub>1</sub></i>)
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static QuaternionD Multiply(QuaternionD q1, QuaternionD q2)
    {
      QuaternionD product;
      product.W = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
      product.X = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
      product.Y = q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X;
      product.Z = q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W;
      return product;

      // An alternative way to write the product:
      // product.W = q1.W * q2.W - Vector3D.Dot(q1.V, q2.V);
      // product.V = Vector3D.Cross(q1.V, q2.V) + q1.W * q2.V + q2.W * q1.V;
    }


    /// <overloads>
    /// <summary>
    /// Divides a quaternion by a scalar or a quaternion.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Divides a quaternion by a scalar.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The quaternion with each component divided by scalar.</returns>
    public static QuaternionD operator /(QuaternionD quaternion, double scalar)
    {
      double f = 1 / scalar;
      quaternion.W *= f;
      quaternion.X *= f;
      quaternion.Y *= f;
      quaternion.Z *= f;
      return quaternion;
    }


    /// <overloads>
    /// <summary>
    /// Divides a quaternion by a scalar or a quaternion.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Divides a quaternion by a scalar.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The quaternion with each component divided by scalar.</returns>
    public static QuaternionD Divide(QuaternionD quaternion, double scalar)
    {
      double f = 1 / scalar;
      quaternion.W *= f;
      quaternion.X *= f;
      quaternion.Y *= f;
      quaternion.Z *= f;
      return quaternion;
    }


    /// <summary>
    /// Divides a quaternions by another quaternion.
    /// </summary>
    /// <param name="dividend">The first quaternion (dividend).</param>
    /// <param name="divisor">The second quaternion (divisor).</param>
    /// <returns>The result of the division.</returns>
    /// <remarks>
    /// <para>
    /// A quaternion is divided by another quaternion by multiplying it with the inverse. For
    /// example, two quaternions <i>q<sub>1</sub></i> and <i>q<sub>2</sub></i>:
    /// </para>
    /// <para>
    /// <i><i>q<sub>1</sub></i> / q<sub>2</sub> = <i>q<sub>1</sub></i>q<sub>2</sub><sup>-1</sup></i>
    /// </para>
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The quaternion <paramref name="divisor"/> cannot be inverted.
    /// </exception>
    public static QuaternionD operator /(QuaternionD dividend, QuaternionD divisor)
    {
      divisor.Invert();
      return dividend * divisor;
    }


    /// <summary>
    /// Divides a quaternions by another quaternion.
    /// </summary>
    /// <param name="dividend">The first quaternion (dividend).</param>
    /// <param name="divisor">The second quaternion (divisor).</param>
    /// <returns>The result of the division.</returns>
    /// <remarks>
    /// <para>
    /// A quaternion is divided by another quaternion by multiplying it with the inverse. For
    /// example, two quaternions <i>q<sub>1</sub></i> and <i>q<sub>2</sub></i>:
    /// </para>
    /// <para>
    /// <i><i>q<sub>1</sub></i> / q<sub>2</sub> = <i>q<sub>1</sub></i>q<sub>2</sub><sup>-1</sup></i>
    /// </para>
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The quaternion <paramref name="divisor"/> cannot be inverted.
    /// </exception>
    public static QuaternionD Divide(QuaternionD dividend, QuaternionD divisor)
    {
      divisor.Invert();
      return dividend * divisor;
    }


    /// <summary>
    /// Tests if two quaternions are equal.
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <returns>
    /// <see langword="true"/> if the quaternions are equal; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the components of the quaternions are compared.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool operator ==(QuaternionD q1, QuaternionD q2)
    {
      return q1.W == q2.W
          && q1.X == q2.X
          && q1.Y == q2.Y
          && q1.Z == q2.Z;
    }


    /// <summary>
    /// Tests if two quaternions are not equal.
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <returns>
    /// <see langword="true"/> if the quaternions are different; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the components of the quaternions are compared.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool operator !=(QuaternionD q1, QuaternionD q2)
    {
      return q1.W != q2.W
          || q1.X != q2.X
          || q1.Y != q2.Y
          || q1.Z != q2.Z;
    }


    /// <overloads>
    /// <summary>
    /// Converts the quaternion to another data type.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the quaternion to an array of 4 <see langword="double"/> values: (w, x, y, z).
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <returns>The array with 4 <see langword="double"/> values (w, x, y, z).</returns>
    public static explicit operator double[](QuaternionD quaternion)
    {
      return new[] { quaternion.W, quaternion.X, quaternion.Y, quaternion.Z };
    }


    /// <summary>
    /// Converts the quaternion to an array of 4 <see langword="double"/> values: (w, x, y, z).
    /// </summary>
    /// <returns>The array with 4 <see langword="double"/> values (w, x, y, z).</returns>
    public double[] ToArray()
    {
      return (double[])this;
    }


    /// <summary>
    /// Converts the vector to a list of 4 <see langword="double"/> values: (w, x, y, z).
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <returns>The list with 4 <see langword="double"/> values (w, x, y, z).</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public static explicit operator List<double>(QuaternionD quaternion)
    {
      List<double> result = new List<double>(4) { quaternion.W, quaternion.X, quaternion.Y, quaternion.Z };
      return result;
    }


    /// <summary>
    /// Converts the vector to a list of 4 <see langword="double"/> values: (w, x, y, z).
    /// </summary>
    /// <returns>The list with 4 <see langword="double"/> values (w, x, y, z).</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public List<double> ToList()
    {
      return (List<double>)this;
    }


    /// <summary>
    /// Performs an explicit conversion from <see cref="QuaternionD"/> to <see cref="QuaternionF"/>.
    /// </summary>
    /// <param name="quaternion">The DigitalRune <see cref="QuaternionD"/>.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator QuaternionF(QuaternionD quaternion)
    {
      return new QuaternionF((float)quaternion.W, (float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z);
    }


    /// <summary>
    /// Converts this <see cref="QuaternionD"/> to <see cref="QuaternionF"/>.
    /// </summary>
    /// <returns>The result of the conversion.</returns>
    public QuaternionF ToQuaternionF()
    {
      return new QuaternionF((float)W, (float)X, (float)Y, (float)Z);
    }


#if XNA || MONOGAME
    /// <summary>
    /// Performs an conversion from <see cref="Quaternion"/> (XNA Framework) to 
    /// <see cref="QuaternionD"/> (DigitalRune Mathematics).
    /// </summary>
    /// <param name="quaternion">The <see cref="Quaternion"/> (XNA Framework).</param>
    /// <returns>The <see cref="QuaternionD"/> (DigitalRune Mathematics).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    public static explicit operator QuaternionD(Quaternion quaternion)
    {
      return new QuaternionD(quaternion.W, quaternion.X, quaternion.Y, quaternion.Z);
    }


    /// <summary>
    /// Converts this <see cref="QuaternionD"/> (DigitalRune Mathematics) to 
    /// <see cref="Quaternion"/> (XNA Framework).
    /// </summary>
    /// <param name="quaternion">The <see cref="Quaternion"/> (XNA Framework).</param>
    /// <returns>The <see cref="QuaternionD"/> (DigitalRune Mathematics).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static QuaternionD FromXna(Quaternion quaternion)
    {
      return new QuaternionD(quaternion.W, quaternion.X, quaternion.Y, quaternion.Z);
    }


    /// <summary>
    /// Performs an conversion from <see cref="QuaternionD"/> (DigitalRune Mathematics) to 
    /// <see cref="Quaternion"/> (XNA Framework).
    /// </summary>
    /// <param name="quaternion">The <see cref="QuaternionD"/> (DigitalRune Mathematics).</param>
    /// <returns>The <see cref="Quaternion"/> (XNA Framework).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    public static explicit operator Quaternion(QuaternionD quaternion)
    {
      return new Quaternion((float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z, (float)quaternion.W);
    }


    /// <summary>
    /// Converts this <see cref="QuaternionD"/> (DigitalRune Mathematics) to 
    /// <see cref="Quaternion"/> (XNA Framework).
    /// </summary>
    /// <returns>The <see cref="Quaternion"/> (XNA Framework).</returns>
    /// <remarks>
    /// This method is available only in the XNA-compatible build of the
    /// DigitalRune.Mathematics.dll.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Quaternion ToXna()
    {
      return new Quaternion((float)X, (float)Y, (float)Z, (float)W);
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Sets this quaternion to its conjugate.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The conjugate of a quaternion is calculated by negating the vector component.
    /// </para>
    /// <para>
    /// <i>q<sup>*</sup> = w - <b>i</b>x - <b>j</b>y - <b>k</b>z</i>
    /// </para>
    /// </remarks>
    public void Conjugate()
    {
      X = -X;
      Y = -Y;
      Z = -Z;
    }


    /// <summary>
    /// Inverts the quaternion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The (multiplicative) inverse of a quaternion is calculated by using the following formula:
    /// </para>
    /// <para>
    /// <i>q<sup>-1</sup> = q<sup>*</sup> / (q q<sup>*</sup>) = q<sup>*</sup> / </i>N(<i>q</i>)
    /// </para>
    /// <para>
    /// The inverse of a unit quaternion is the same as its conjugate. You might consider using the
    /// method <see cref="Conjugate"/> because it is faster than <see cref="Invert"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The length of the quaternion is zero. The quaternion cannot be inverted.
    /// </exception>
    public void Invert()
    {
      double norm = Norm;
      if (Numeric.IsZero(norm, Numeric.EpsilonDSquared))
        throw new MathematicsException("The length of the quaternion is zero. The quaternion cannot be inverted.");

      double scale = 1 / norm;
      W *= scale;
      X = -X * scale;
      Y = -Y * scale;
      Z = -Z * scale;
    }


    /// <summary>
    /// Normalizes the quaternion.
    /// </summary>
    /// <remarks>
    /// A quaternion is normalized by dividing its components by the length of the quaternion.
    /// </remarks>
    /// <exception cref="DivideByZeroException">
    /// The length of the quaternion is zero. The quaternion cannot be normalized.
    /// </exception>
    public void Normalize()
    {
      double norm = Norm;
      if (Numeric.IsZero(norm))
        throw new DivideByZeroException("Cannot normalize a quaternion with a length of 0.");

      double scale = 1.0 / Math.Sqrt(norm);
      W *= scale;
      X *= scale;
      Y *= scale;
      Z *= scale;
    }


    /// <summary>
    /// Tries to normalize the quaternion.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the quaternion was normalized; otherwise, <see langword="false"/> 
    /// if the quaternion could not be normalized. (The norm is numerically zero.)
    /// </returns>
    public bool TryNormalize()
    {
      double norm = Norm;
      if (Numeric.IsZero(norm, Numeric.EpsilonDSquared))
        return false;

      double scale = 1.0 / Math.Sqrt(norm);
      W *= scale;
      X *= scale;
      Y *= scale;
      Z *= scale;

      return true;
    }


    /// <overloads>
    /// <summary>
    /// Calculates the exponential.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets this quaternion to its exponential.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This method requires that the quaternion is a pure quaternion. A
    /// pure quaternion is defined by <i>q</i> = (0, <i><b>u</b>θ</i>) where <i><b>u</b></i> is a
    /// unit vector.
    /// </para>
    /// <para>
    /// The exponential of a quaternion <i>q</i> is defines as:
    /// </para>
    /// <para>
    /// e<sup><i>q</i></sup> = (cos(<i>θ</i>) + <i><b>u</b></i>sin(<i>θ</i>))
    /// </para>
    /// <para>
    /// The result is returned as a quaternion with the form:
    /// (cos(<i>θ</i>), <i><b>u</b></i>sin(<i>θ</i>))
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Exp()
    {
      this = Exp(this);
    }


    /// <overloads>
    /// <summary>
    /// Calculates the natural logarithm.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets this quaternion to its natural logarithm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This method requires that the quaternion is a unit quaternion.
    /// </para>
    /// <para>
    /// The natural logarithm of a quaternion <i>q</i> is defines as:
    /// </para>
    /// <para>
    /// ln(<i>q</i>) = ln(cos(<i>θ</i>) + <i><b>u</b></i>sin(<i>θ</i>)) 
    ///              = ln(e<sup><i><b>u</b>θ</i></sup>) = <i><b>u</b>θ</i>
    /// </para>
    /// <para>
    /// The result is returned as a quaternion with the form: (0, <i><b>u</b>θ</i>)
    /// </para>
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The quaternion is not a unit quaternion.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
    public void Ln()
    {
      this = Ln(this);
    }


    /// <overloads>
    /// <summary>
    /// Calculates the power of a unit quaternion.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets this unit quaternion to a power of itself.
    /// </summary>
    /// <param name="t">The exponent.</param>
    /// <returns>The power of the unit quaternion.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This method requires that the quaternion is a unit quaternion.
    /// </para>
    /// <para>
    /// The power of quaternion is defined as:
    /// </para>
    /// <para>
    /// <i>q<sup>t</sup></i> = e<sup><i><b>u</b>tθ</i></sup> 
    ///                      = cos(<i>tθ</i>) + <i><b>u</b></i>sin(<i>tθ</i>)
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Power(double t)
    {
      this = Power(this, t);
    }


    /// <summary>
    /// Rotates a vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    /// <returns>The rotated vector.</returns>
    /// <remarks>
    /// <para>
    /// The rotation of a vector <i>v</i> by quaternion <i>q</i> is defined as:
    /// </para>
    /// <para>
    /// <i>(0, <i>v'</i>)</i> = <i>q</i> * (0, <i>v</i>) * <i>q</i><sup>-1</sup>
    /// </para>
    /// </remarks>
    public Vector3D Rotate(Vector3D vector)
    {
      // ----- Matrix version
      // return RotationMatrix33 * vector;

      // ----- Quaternion algebra version
      //return (this * new QuaternionD(0, vector) * Inverse).V;

      // ----- Optimized version #1
      // The W component of the vector is zero, so we can simplify the first 
      // quaternion multiplication. And, we do not need to compute the final w
      // component.

      // First multiplication: 
      //   this * new QuaternionD(0, vector)

      //double w = -Vector3D.Dot(V, vector);

      //Vector3D v = Vector3D.Cross(V, vector) + W * vector;

      //// Second multiplication (vector component only): 
      ////   q * Inverse
      //Vector3D inverse;
      //inverse.X = -X;
      //inverse.Y = -Y;
      //inverse.Z = -Z;

      //v = Vector3D.Cross(v, inverse) + w * inverse + W * v;
      //return v;

      // ----- Optimized version #2
      // Derivation by Fabian Giesen on Molly Rocket forum.
      // See http://mollyrocket.com/forums/molly_forum_833.html

      // t = 2 * cross(q.xyz, v)
      // v' = v + q.w * t + cross(q.xyz, t)
      double qX = X;
      double qY = Y;
      double qZ = Z;
      double qW = W;
      double tX = 2 * (qY * vector.Z - qZ * vector.Y);
      double tY = 2 * (qZ * vector.X - qX * vector.Z);
      double tZ = 2 * (qX * vector.Y - qY * vector.X);
      return new Vector3D(
        vector.X + qW * tX + (qY * tZ - qZ * tY),
        vector.Y + qW * tY + (qZ * tX - qX * tZ),
        vector.Z + qW * tZ + (qX * tY - qY * tX));
    }


    /// <summary>
    /// Returns the 3 x 3 rotation matrix of this quaternion.
    /// </summary>
    /// <returns>The rotation matrix.</returns>
    /// <remarks>
    /// The method assumes that this quaternion is a unit quaternion (i.e. that it is normalized).
    /// The unit quaternion specifies a rotation that can be converted into a corresponding 3 x 3
    /// rotation matrix.
    /// </remarks>
    public Matrix33D ToRotationMatrix33()
    {
      Matrix33D m;

      double twoX = 2 * X;
      double twoY = 2 * Y;
      double twoZ = 2 * Z;
      double twoXX = twoX * X;
      double twoYY = twoY * Y;
      double twoZZ = twoZ * Z;
      double twoXY = twoX * Y;
      double twoXZ = twoX * Z;
      double twoYZ = twoY * Z;
      double twoXW = twoX * W;
      double twoYW = twoY * W;
      double twoZW = twoZ * W;

      // according to Watt, p.489
      m.M00 = 1 - (twoYY + twoZZ);
      m.M01 = twoXY - twoZW;
      m.M02 = twoYW + twoXZ;

      m.M10 = twoXY + twoZW;
      m.M11 = 1 - (twoXX + twoZZ);
      m.M12 = twoYZ - twoXW;

      m.M20 = twoXZ - twoYW;
      m.M21 = twoXW + twoYZ;
      m.M22 = 1 - (twoXX + twoYY);

      return m;
    }


    /// <summary>
    /// Returns the 4 x 4 rotation matrix of this quaternion.
    /// </summary>
    /// <returns>The rotation matrix.</returns>
    /// <remarks>
    /// <para>
    /// The method assumes that this quaternion is a unit quaternion (i.e. that it is normalized).
    /// The unit quaternion specifies a rotation that can be converted into a corresponding rotation
    /// matrix.
    /// </para>
    /// <para>
    /// The resulting 4 x 4 matrix specifies a 3-dimensional rotation in the homogeneous coordinate
    /// space. The translation part of the matrix is set to (0, 0, 0).
    /// </para>
    /// </remarks>
    public Matrix44D ToRotationMatrix44()
    {
      Matrix44D m = new Matrix44D();

      double twoX = 2 * X;
      double twoY = 2 * Y;
      double twoZ = 2 * Z;
      double twoXX = twoX * X;
      double twoYY = twoY * Y;
      double twoZZ = twoZ * Z;
      double twoXY = twoX * Y;
      double twoXZ = twoX * Z;
      double twoYZ = twoY * Z;
      double twoXW = twoX * W;
      double twoYW = twoY * W;
      double twoZW = twoZ * W;

      // according to Watt, p.489
      m.M00 = 1 - (twoYY + twoZZ);
      m.M01 = twoXY - twoZW;
      m.M02 = twoYW + twoXZ;
      m.M03 = 0;

      m.M10 = twoXY + twoZW;
      m.M11 = 1 - (twoXX + twoZZ);
      m.M12 = twoYZ - twoXW;
      m.M13 = 0;

      m.M20 = twoXZ - twoYW;
      m.M21 = twoXW + twoYZ;
      m.M22 = 1 - (twoXX + twoYY);
      m.M23 = 0;

      m.M30 = 0;
      m.M31 = 0;
      m.M32 = 0;
      m.M33 = 1;

      return m;
    }
    #endregion


    //--------------------------------------------------------------
    #region Static Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Determines whether two quaternions are equal (regarding a given tolerance).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Tests if two quaternions are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonD"/>).
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <returns>
    /// <see langword="true"/> if the quaternions are equal within the tolerance 
    /// <see cref="Numeric.EpsilonD"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the components of the quaternions are compared.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool AreNumericallyEqual(QuaternionD q1, QuaternionD q2)
    {
      return Numeric.AreEqual(q1.W, q2.W)
          && Numeric.AreEqual(q1.X, q2.X)
          && Numeric.AreEqual(q1.Y, q2.Y)
          && Numeric.AreEqual(q1.Z, q2.Z);
    }


    /// <summary>
    /// Tests if two quaternions are equal (with a specific tolerance).
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>
    /// <see langword="true"/> if the quaternions are equal within the tolerance 
    /// <paramref name="epsilon"/>; otherwise <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// For the test the components of the quaternions are compared.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool AreNumericallyEqual(QuaternionD q1, QuaternionD q2, double epsilon)
    {
      return Numeric.AreEqual(q1.W, q2.W, epsilon)
          && Numeric.AreEqual(q1.X, q2.X, epsilon)
          && Numeric.AreEqual(q1.Y, q2.Y, epsilon)
          && Numeric.AreEqual(q1.Z, q2.Z, epsilon);
    }


    /// <summary>
    /// Returns the dot product of two quaternions.
    /// </summary>
    /// <param name="q1">The first quaternion.</param>
    /// <param name="q2">The second quaternion.</param>
    /// <returns>The dot product of the two quaternions.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static double Dot(QuaternionD q1, QuaternionD q2)
    {
      return q1.W * q2.W + q1.X * q2.X + q1.Y * q2.Y + q1.Z * q2.Z;
    }


    /// <summary>
    /// Creates a unit quaternion that specifies a rotation given by axis and angle.
    /// </summary>
    /// <param name="axis">The axis. (Vector does not need to be normalized.)</param>
    /// <param name="angle">The angle.</param>
    /// <returns>
    /// <para>
    /// The created unit quaternion that describes a rotation by the 
    /// <paramref name="angle"/> radians around the <paramref name="axis"/>.
    /// (<paramref name="axis"/> will be normalized automatically.)
    /// </para>
    /// <para>
    /// The resulting quaternion is: <i>q</i> = (cos(<i>θ</i>/2), <i><b>v</b></i>sin(<i>θ</i>/2))
    /// </para>
    /// <para>
    /// <i>q</i> = (cos(<i>θ</i>/2), <i><b>v</b></i>sin(<i>θ</i>/2))
    /// </para>
    /// where <i>θ</i> is the angle and <i><b>v</b></i> is the normalized axis.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="axis"/> vector has 0 length.
    /// </exception>
    public static QuaternionD CreateRotation(Vector3D axis, double angle)
    {
      if (!axis.TryNormalize())
        throw new ArgumentException("The axis vector has length 0.");

      QuaternionD q;
      double halfangle = angle / 2.0;

      // W = cos(angle / 2);
      q.W = Math.Cos(halfangle);

      // V = sin(angle / 2) * axis
      double sin = Math.Sin(halfangle);
      q.X = sin * axis.X;
      q.Y = sin * axis.Y;
      q.Z = sin * axis.Z;

      return q;
    }


    /// <summary>
    /// Creates a unit quaternion that specifies a rotation by a given angle around the x-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>
    /// The created unit quaternion that describes a rotation by the <paramref name="angle"/>
    /// radians around the x-axis.
    /// </returns>
    public static QuaternionD CreateRotationX(double angle)
    {
      QuaternionD q;
      double halfangle = angle / 2.0;

      // W = cos(angle / 2);
      q.W = Math.Cos(halfangle);

      // V = sin(angle / 2) * axis
      q.X = Math.Sin(halfangle);
      q.Y = 0;
      q.Z = 0;

      return q;
    }


    /// <summary>
    /// Creates a unit quaternion that specifies a rotation by a given angle around the y-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>
    /// The created unit quaternion that describes a rotation by the <paramref name="angle"/>
    /// radians around the y-axis.
    /// </returns>
    public static QuaternionD CreateRotationY(double angle)
    {
      QuaternionD q;
      double halfangle = angle / 2.0;

      // W = cos(angle / 2);
      q.W = Math.Cos(halfangle);

      // V = sin(angle / 2) * axis
      q.X = 0;
      q.Y = Math.Sin(halfangle);
      q.Z = 0;

      return q;
    }


    /// <summary>
    /// Creates a unit quaternion that specifies a rotation by a given angle around the z-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>
    /// The created unit quaternion that describes a rotation by the <paramref name="angle"/>
    /// radians around the z-axis.
    /// </returns>
    public static QuaternionD CreateRotationZ(double angle)
    {
      QuaternionD q;
      double halfangle = angle / 2.0;

      // W = cos(angle / 2);
      q.W = Math.Cos(halfangle);

      // V = sin(angle / 2) * axis
      q.X = 0;
      q.Y = 0;
      q.Z = Math.Sin(halfangle);

      return q;
    }


    /// <summary>
    /// Creates a unit quaternion that specifies the same rotation as the given rotation matrix.
    /// </summary>
    /// <param name="rotationMatrix">A orientation matrix that specifies a rotation.</param>
    /// <returns>
    /// The creates unit quaternion that describes the same rotation as the rotation matrix.
    /// </returns>
    /// <remarks>
    /// The given matrix is converted into a unit quaternion that specifies the same rotation.
    /// </remarks>
    public static QuaternionD CreateRotation(Matrix33D rotationMatrix)
    {
      // Credits: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
      // Note: A less general branchless implementation is discussed here: http://www.thetenthplanet.de/archives/1994

      double x, y, z, w;

      // Calculate diagonal sum (= trace) of the matrix.
      double trace = rotationMatrix.M00 + rotationMatrix.M11 + rotationMatrix.M22;
      if (trace > 0)
      {
        double s = 0.5 / Math.Sqrt(trace + 1.0);
        w = 0.25 / s;
        x = (rotationMatrix.M21 - rotationMatrix.M12) * s;
        y = (rotationMatrix.M02 - rotationMatrix.M20) * s;
        z = (rotationMatrix.M10 - rotationMatrix.M01) * s;
      }
      else
      {
        if (rotationMatrix.M00 > rotationMatrix.M11 && rotationMatrix.M00 > rotationMatrix.M22)
        {
          double s = 2.0 * Math.Sqrt(1.0 + rotationMatrix.M00 - rotationMatrix.M11 - rotationMatrix.M22);
          w = (rotationMatrix.M21 - rotationMatrix.M12) / s;
          x = 0.25 * s;
          y = (rotationMatrix.M01 + rotationMatrix.M10) / s;
          z = (rotationMatrix.M02 + rotationMatrix.M20) / s;
        }
        else if (rotationMatrix.M11 > rotationMatrix.M22)
        {
          double s = 2.0 * Math.Sqrt(1.0 + rotationMatrix.M11 - rotationMatrix.M00 - rotationMatrix.M22);
          w = (rotationMatrix.M02 - rotationMatrix.M20) / s;
          x = (rotationMatrix.M01 + rotationMatrix.M10) / s;
          y = 0.25 * s;
          z = (rotationMatrix.M12 + rotationMatrix.M21) / s;
        }
        else
        {
          double s = 2.0 * Math.Sqrt(1.0 + rotationMatrix.M22 - rotationMatrix.M00 - rotationMatrix.M11);
          w = (rotationMatrix.M10 - rotationMatrix.M01) / s;
          x = (rotationMatrix.M02 + rotationMatrix.M20) / s;
          y = (rotationMatrix.M12 + rotationMatrix.M21) / s;
          z = 0.25 * s;
        }
      }

      return new QuaternionD(w, x, y, z);
    }


    /// <summary>
    /// Creates a unit quaternion that specifies a rotation given by two vectors.
    /// </summary>
    /// <param name="startVector">
    /// The initial vector. (Vector does not need to be normalized.)
    /// </param>
    /// <param name="rotatedVector">
    /// The rotated vector. (Vector does not need to be normalized.)
    /// </param>
    /// <returns>
    /// The created unit quaternion that would rotate <paramref name="startVector"/> to 
    /// <paramref name="rotatedVector"/>.
    /// </returns>
    /// <remarks>
    /// The quaternion is set to a rotation that would rotate vector <c>startVector</c> to the
    /// orientation of vector <c>rotatedVector</c>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// The length of the <paramref name="startVector"/> and <paramref name="rotatedVector"/> must
    /// not be <c>0</c>.
    /// </exception>
    public static QuaternionD CreateRotation(Vector3D startVector, Vector3D rotatedVector)
    {
      // An optimized version, which does not handle degenerate cases, is discussed here: 
      // - Beautiful maths simplification: quaternion from two vectors
      //   http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

      if (!startVector.TryNormalize())
        throw new ArgumentException("Length of the start vector must not be 0.");
      if (!rotatedVector.TryNormalize())
        throw new ArgumentException("Length of the rotated vector must not be 0.");

      Vector3D axis = Vector3D.Cross(startVector, rotatedVector);
      double cosθ = Vector3D.Dot(startVector, rotatedVector);
      double x, y, z, w;

      // Special case:
      // When the axes are parallel with opposite directions, then cosθ is close to -1.
      // This would cause a division by zero later on - so we make a shortcut here:
      if (Numeric.AreEqual(cosθ, -1.0))
      {
        w = 0.0;

        // any axis normal to startVector will do
        axis = startVector.Orthonormal1;
        x = axis.X;
        y = axis.Y;
        z = axis.Z;
        return new QuaternionD(w, x, y, z);
      }

      // W needs to be cos(θ/2). This can be calculated by the following operation.
      double factor = Math.Sqrt(2 * (cosθ + 1));
      w = factor / 2;

      // axis has the length of sinθ. We need to scale the axis, such that it 
      // has the length sin(θ/2). This can be done by the following operation.
      // (See "DigitalRune - Knowledge Base (Math Tricks)" for more info.)
      axis /= factor;
      x = axis.X;
      y = axis.Y;
      z = axis.Z;
      return new QuaternionD(w, x, y, z);

      #region ----- Slow version -----
      /*
      if (!Numeric.IsZero(axis.LengthSquared))
      {
        double sinTheta = axis.Length;
        double cosTheta = Vector3D.Dot(startVector, rotatedVector);
        double angle;
        if (cosTheta >= 0.0)
          angle = Math.Asin(sinTheta);            // 1st or 4th quadrant
        else
          angle = Math.PI - Math.Asin(sinTheta);  // 2nd or 3rd quadrant
        axis.Normalize();
        double halfangle = angle / 2;
        W = Math.Cos(halfangle);
        X = Math.Sin(halfangle) * axis.X;
        Y = Math.Sin(halfangle) * axis.Y;
        Z = Math.Sin(halfangle) * axis.Z;
      }
      else
      {
        // Vectors are parallel
        if (Vector3D.AreNumericallyEqual(startVector, rotatedVector))
        {
          // rotation of 0° (same direction)
          W = 1.0;
          X = 0;
          Y = 0;
          Z = 0;
        }
        else
        {
          // rotation of 180° (opposite direction)
          W = 0.0;

          // any axis normal to startVector will do
          axis = startVector.Orthonormal1;
          X = axis.X;
          Y = axis.Y;
          Z = axis.Z;
        }
      }
      */
      #endregion
    }


    /// <summary>
    /// Calculates the exponential.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <returns>The exponential e<sup><i>q</i></sup>.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This method requires that the quaternion is a pure quaternion. A
    /// pure quaternion is defined by <i>q</i> = (0, <i><b>u</b>θ</i>) where <i><b>u</b></i> is a
    /// unit vector.
    /// </para>
    /// <para>
    /// The exponential of a quaternion <i>q</i> is defines as:
    /// </para>
    /// <para>
    /// e<sup><i>q</i></sup> = (cos<i>θ</i> + <i><b>u</b></i>sin<i>θ</i>)
    /// </para>
    /// <para>
    /// The result is returned as a quaternion with the form: 
    /// (cos(<i>θ</i>), <i><b>u</b></i>sin(<i>θ</i>))
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static QuaternionD Exp(QuaternionD quaternion)
    {
      double θ = Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
      double cosθ = Math.Cos(θ);

      if (θ > Numeric.EpsilonD)
      {
        double coefficient = Math.Sin(θ) / θ;
        quaternion.W = cosθ;
        quaternion.X *= coefficient;
        quaternion.Y *= coefficient;
        quaternion.Z *= coefficient;
      }
      else
      {
        // In this case θ was 0.
        // Therefore: cos(θ) = 1, sin(θ) = 0
        quaternion.W = cosθ;

        // We do not have to set (x, y, z) because we already know that length
        // is 0.
      }
      return quaternion;
    }


    /// <overloads>
    /// <summary>
    /// Creates a quaternion for a given rotation.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets an orientation quaternion from Euler angles (3 rotations around 3 axes).
    /// </summary>
    /// <param name="angle1">The first angle.</param>
    /// <param name="axis1">The first axis.</param>
    /// <param name="angle2">The second angle.</param>
    /// <param name="axis2">The second axis.</param>
    /// <param name="angle3">The third angle.</param>
    /// <param name="axis3">The third axis.</param>
    /// <param name="useGlobalAxes">
    /// If set to <see langword="true"/> then the rotation axes are fixed in world space. Otherwise 
    /// the rotation axes are fixed on the object and rotated with each rotation.
    /// </param>
    /// <remarks>
    /// A rotation is created from 3 sequential rotations. Each rotation is defined by an angle and 
    /// the rotation axis. This method can be used to create a quaternion from Euler angle 
    /// representations, often named Azimuth/Elevation/Roll, or Heading/Pitch/Roll.
    /// </remarks>
    /// <returns>
    /// The orientation quaternion that describes the same orientation as the given Euler angles.
    /// </returns>
    /// <exception cref="MathematicsException">
    /// The length of the axis vectors must not be <c>0</c>.
    /// </exception>
    public static QuaternionD CreateRotation(double angle1, Vector3D axis1, double angle2, Vector3D axis2,
      double angle3, Vector3D axis3, bool useGlobalAxes)
    {
      QuaternionD rotation1 = CreateRotation(axis1, angle1);
      QuaternionD rotation2 = CreateRotation(axis2, angle2);
      QuaternionD rotation3 = CreateRotation(axis3, angle3);

      if (useGlobalAxes)
        return rotation3 * rotation2 * rotation1;
      else
        return rotation1 * rotation2 * rotation3;
    }


    /// <summary>
    /// Calculates the natural logarithm.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <returns>The natural logarithm ln(<i>q</i>).</returns>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This method requires that the quaternion is a unit quaternion.
    /// </para>
    /// <para>
    /// The natural logarithm of a quaternion <i>q</i> is defines as:
    /// </para>
    /// <para>
    /// ln(<i>q</i>) = ln(cos(<i>θ</i>) + <i><b>u</b></i>sin(<i>θ</i>)) 
    ///              = ln(e<sup><i><b>u</b>θ</i></sup>) = <i><b>u</b>θ</i>
    /// </para>
    /// <para>
    /// The result is returned as a quaternion with the form: (0, <i><b>u</b>θ</i>)
    /// </para>
    /// </remarks>
    /// <exception cref="MathematicsException">
    /// The given quaternion is not a unit quaternion.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
    public static QuaternionD Ln(QuaternionD quaternion)
    {
      if (Numeric.IsLessOrEqual(Math.Abs(quaternion.W), 1.0))
      {
        double sinθ = Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
        double θ = Math.Atan(sinθ / quaternion.W);

        // Slower version:
        //double θ = System.Math.Acos(quaternion.W);
        //double sinθ = System.Math.Sin(θ);

        if (!Numeric.IsZero(sinθ))
        {
          double coefficient = θ / sinθ;
          quaternion.W = 0.0;
          quaternion.X *= coefficient;
          quaternion.Y *= coefficient;
          quaternion.Z *= coefficient;
        }
        else
        {
          // In this case θ was 0.
          // cos(θ) = 1, sin(θ) = 0
          // We assume that the given quaternion is a unit quaternion.
          // If w = 1, then all other components should be 0.
          Debug.Assert(Numeric.IsZero(quaternion.X), "Quaternion is not a unit quaternion.");
          Debug.Assert(Numeric.IsZero(quaternion.Y), "Quaternion is not a unit quaternion.");
          Debug.Assert(Numeric.IsZero(quaternion.Z), "Quaternion is not a unit quaternion.");

          // Return (0, (0, 0, 0))
          quaternion.W = 0.0;

          // We do not have to touch (x, y, z).
        }
        return quaternion;
      }
      else
      {
        throw new MathematicsException("The quaternion is not a unit quaternion. Ln only works for unit quaternions.");
      }
    }


    /// <summary>
    /// Calculates the power of a unit quaternion.
    /// </summary>
    /// <param name="quaternion">The quaternion.</param>
    /// <param name="t">The exponent.</param>
    /// <returns>The power of the unit quaternion.</returns>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> This method requires that the quaternion is a unit quaternion.
    /// </para>
    /// <para>
    /// The power of quaternion is defined as:
    /// </para>
    /// <para>
    /// <i>q<sup>t</sup></i> = e<sup><i><b>u</b>tθ</i></sup> 
    ///                      = cos(<i>tθ</i>) + <i><b>u</b></i>sin(<i>tθ</i>)
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static QuaternionD Power(QuaternionD quaternion, double t)
    {
      return Exp(Ln(quaternion) * t);
    }


    /// <summary>
    /// Calculates the angle between two quaternions.
    /// </summary>
    /// <param name="quaternion1">The first quaternion.</param>
    /// <param name="quaternion2">The second quaternion.</param>
    /// <returns>The angle between the given vectors, such that 0 ≤ angle ≤ π.</returns>
    /// <remarks>
    /// <para>
    /// The quaternions are interpreted as orientations. The result is the angle of the quaternion
    /// which would rotate an object in the first orientation to the second orientation.
    /// </para>
    /// <para>
    /// The result is only valid for unit quaternions.
    /// </para>
    /// </remarks>
    public static double GetAngle(QuaternionD quaternion1, QuaternionD quaternion2)
    {
      double α = Dot(quaternion1, quaternion2);

      // Inaccuracy in the floating-point operations can cause
      // the result be outside of the valid range.
      // Ensure that the dot product α lies in the interval [-1, 1].
      // Math.Acos() returns Double.NaN if the argument lies outside
      // of this interval.
      α = MathHelper.Clamp(α, -1.0, 1.0);
      if (α < 0)
        α *= -1;

      return 2.0 * Math.Acos(α);
    }


    /// <overloads>
    /// <summary>
    /// Converts the string representation of a quaternion to its <see cref="QuaternionD"/> 
    /// equivalent.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the string representation of a quaternion to its <see cref="QuaternionD"/> 
    /// equivalent.
    /// </summary>
    /// <param name="s">A string representation of a 4-dimensional vector.</param>
    /// <returns>
    /// A <see cref="QuaternionD"/> that represents the vector specified by the <paramref name="s"/>
    /// parameter.
    /// </returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid <see cref="QuaternionD"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static QuaternionD Parse(string s)
    {
      return Parse(s, CultureInfo.CurrentCulture);
    }


    /// <summary>
    /// Converts the string representation of a quaternion in a specified culture-specific format to
    /// its <see cref="QuaternionD"/> equivalent.
    /// </summary>
    /// <param name="s">A string representation of a 4-dimensional vector.</param>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information about
    /// <paramref name="s"/>. 
    /// </param>
    /// <returns>
    /// A <see cref="QuaternionD"/> that represents the vector specified by the <paramref name="s"/>
    /// parameter.
    /// </returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not a valid <see cref="QuaternionD"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public static QuaternionD Parse(string s, IFormatProvider provider)
    {
      Match m = Regex.Match(s, @"\((?<w>.*);\s*\((?<x>.*);(?<y>.*);(?<z>.*)\)\s*\)", RegexOptions.None);
      if (m.Success)
      {
        return new QuaternionD(double.Parse(m.Groups["w"].Value, provider),
          double.Parse(m.Groups["x"].Value, provider),
          double.Parse(m.Groups["y"].Value, provider),
          double.Parse(m.Groups["z"].Value, provider));
      }

      throw new FormatException("String is not a valid QuaternionD.");
    }
    #endregion
  }
}
