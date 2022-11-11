using System;
using Sandbox;

namespace Interfacer;

/// <summary>
/// A two component vector of integers.
/// </summary>
public struct IntVector
{
	public static implicit operator Vector2( IntVector vec )
	{
		return new Vector2( vec.x, vec.y );
	}

	public static explicit operator IntVector( Vector2 vec )
	{
		return new IntVector( (int)vec.x, (int)vec.y );
	}

	/// <summary>
	/// A vector with zero for all components.
	/// </summary>
	public static readonly IntVector Zero = new IntVector( 0, 0 );

	/// <summary>
	/// A normalized vector along the positive X axis.
	/// </summary>
	public static readonly IntVector UnitX = new IntVector( 1, 0 );

	/// <summary>
	/// A normalized vector along the positive Y axis.
	/// </summary>
	public static readonly IntVector UnitY = new IntVector( 0, 1 );

	#region Operators
	/// <summary>
	/// The identity operator.
	/// </summary>
	public static IntVector operator +( IntVector vec )
	{
		return vec;
	}

	/// <summary>
	/// Component-wise addition of a vector to another.
	/// </summary>
	public static IntVector operator +( IntVector a, IntVector b )
	{
		return new IntVector( a.x + b.x, a.y + b.y );
	}

	/// <summary>
	/// Finds the negation of a vector.
	/// </summary>
	public static IntVector operator -( IntVector vec )
	{
		return new IntVector( -vec.x, -vec.y );
	}

	/// <summary>
	/// Component-wise subtraction of a vector from another.
	/// </summary>
	public static IntVector operator -( IntVector a, IntVector b )
	{
		return new IntVector( a.x - b.x, a.y - b.y );
	}

	/// <summary>
	/// Component-wise multiplication of a vector by another.
	/// </summary>
	public static IntVector operator *( IntVector a, IntVector b )
	{
		return new IntVector( a.x * b.x, a.y * b.y );
	}

	/// <summary>
	/// Multiplies a vector by a scalar.
	/// </summary>
	public static IntVector operator *( IntVector vec, int val )
	{
		return new IntVector( vec.x * val, vec.y * val );
	}

	/// <summary>
	/// Multiplies a vector by a scalar.
	/// </summary>
	public static Vector2 operator *( IntVector vec, float val )
	{
		return new Vector2( vec.x * val, vec.y * val );
	}

	/// <summary>
	/// Multiplies a vector by a scalar.
	/// </summary>
	public static IntVector operator *( int val, IntVector vec )
	{
		return new IntVector( vec.x * val, vec.y * val );
	}

	/// <summary>
	/// Multiplies a vector by a scalar.
	/// </summary>
	public static Vector2 operator *( float val, IntVector vec )
	{
		return new Vector2( vec.x * val, vec.y * val );
	}

	/// <summary>
	/// Component-wise division of a vector by another.
	/// </summary>
	public static IntVector operator /( IntVector a, IntVector b )
	{
		return new IntVector( a.x / b.x, a.y / b.y );
	}

	/// <summary>
	/// Component-wise division of a vector by another.
	/// </summary>
	public static Vector2 operator /( Vector2 a, IntVector b )
	{
		return new Vector2( a.x / b.x, a.y / b.y );
	}

	/// <summary>
	/// Component-wise division of a vector by another.
	/// </summary>
	public static Vector2 operator /( IntVector a, Vector2 b )
	{
		return new Vector2( a.x / b.x, a.y / b.y );
	}

	/// <summary>
	/// Division of this vector by a scalar.
	/// </summary>
	public static IntVector operator /( IntVector vec, int val )
	{
		return new IntVector( vec.x / val, vec.y / val );
	}

	public static Vector2 operator /( IntVector vec, float val )
	{
		return new Vector2( vec.x / val, vec.y / val );
	}

	public static IntVector operator /( int val, IntVector vec )
	{
		return new IntVector( val / vec.x, val / vec.y );
	}

	public static Vector2 operator /( float val, IntVector vec )
	{
		return new Vector2( val / vec.x, val / vec.y );
	}
	#endregion

	/// <summary>
	/// Horizontal component.
	/// </summary>
	public int x;

	/// <summary>
	/// Vertical component.
	/// </summary>
	public int y;

	/// <summary>
	/// Floating point magnitude of the vector.
	/// </summary>
	public float Length { get { return (float)MathF.Sqrt( x * x + y * y ); } }

	/// <summary>
	/// Magnitude of the vector in Taxicab geometry.
	/// </summary>
	public int ManhattanLength { get { return (int)MathF.Abs( x ) + (int)MathF.Abs( y ); } }

	/// <summary>
	/// Sum of each component squared.
	/// </summary>
	public int LengthSquared { get { return x * x + y * y; } }

	/// <summary>
	/// Gets a normalized vector in the same direction as this one.
	/// </summary>
	public Vector2 Normalized { get { return LengthSquared >= float.Epsilon ? this / Length : Zero; } }

	/// <summary>
	/// Gets a vector equal to this one rotated counter-clockwise by 90 degrees.
	/// </summary>
	public IntVector Left { get { return new IntVector( -y, x ); } }

	/// <summary>
	/// Gets a vector equal to this one rotated clockwise 90 degrees.
	/// </summary>
	public IntVector Right { get { return new IntVector( y, -x ); } }

	/// <summary>
	/// Gets a vector equal to this one rotated 180 degrees.
	/// </summary>
	public IntVector Back { get { return -this; } }

	/// <summary>
	/// Constructs a vector from the given X and Y components.
	/// </summary>
	public IntVector( int x, int y )
	{
		this.x = x; this.y = y;
	}

	/// <summary>
	/// Finds the scalar product of this vector and another.
	/// </summary>
	public int Dot( IntVector vec )
	{
		return x * vec.x + y * vec.y;
	}

	public override bool Equals( object obj )
	{
		return obj is IntVector && Equals( (IntVector)obj );
	}

	/// <summary>
	/// Tests for equality with another vector.
	/// </summary>
	public bool Equals( IntVector vec )
	{
		return x == vec.x && y == vec.y;
	}

	public override int GetHashCode()
	{
		return x ^ y;
	}

	/// <summary>
	/// Gets a string representing this vector in (X, Y) format.
	/// </summary>
	public override string ToString()
	{
		return string.Format( "({0}, {1})", x, y );
	}
}
