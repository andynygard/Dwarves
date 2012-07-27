// ----------------------------------------------------------------------------
// <copyright file="RectI.cs" company="Acidwashed Games">
//     Copyright 2012 Acidwashed Games. All right reserved.
// </copyright>
// ----------------------------------------------------------------------------

/// <summary>
/// An integer-based rectangle.
/// </summary>
public struct RectI
{
    /// <summary>
    /// An empty rectangle.
    /// </summary>
	public const RectI Empty = new RectI(0, 0, 0, 0);

	#region Constructor
	
    /// <summary>
    /// Initializes a new instance of the Vector2I struct.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y posititon.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public RectI(int x, int y, int width, int height)
        : this()
    {
        this.X = x;
        this.Y = y;
		this.Width = width;
		this.Height = height;
    }
	
	#endregion
	
	#region Properties

    /// <summary>
    /// Gets or sets the x position.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the y position.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public int Height { get; set; }

	/// <summary>
	/// Gets the y-coordinate of the bottom side of the rectangle that is not contained in the rectangle.
	/// </summary>
	public int Bottom
	{
		get
		{
			return this.Y + this.Height;
		}
	}

	/// <summary>
	/// Gets the x-coordinate of the right side of the rectangle that is not contained in the rectangle.
	/// </summary>
	public int Right
	{
		get
		{
			return this.X + this.Width;
		}
	}
	
	/// <summary>
	/// Gets or sets the top left position of the rectangle.
	/// </summary>
	public Vector2I Position
	{
		get
		{
			return new Vector2I(this.X, this.Y);
		}

		set
		{
			this.X = value.X;
			this.Y = value.Y;
		}
	}

	/// <summary>
	/// Gets the center position of the rectangle.
	/// </summary>
	public Vector2I Center
	{
		get
		{
			return new Vector2I(
				this.X + (int)((this.Width - 1) / 2),
				this.Y + (int)((this.Height - 1) / 2));
		}
	}
	
	#endregion

	#region Method Overrides

	/// <summary>
	/// Gets a value indicating whether the two rectangles are equal.
	/// </summary>
	/// <param name="r1">The first rectangle.</param>
	/// <param name="r2">The second rectangle.</param>
	/// <returns>True if the rectangles are equal.</returns>
	public static bool operator ==(RectI r1, RectI r2)
	{
		return r1.X == r2.X && r1.Y == r2.Y && r1.Width == r2.Width && r1.Height == r2.Height;
	}

	/// <summary>
	/// Gets a value indicating whether the two rectangles are not equal.
	/// </summary>
	/// <param name="r1">The first rectangle.</param>
	/// <param name="r2">The second rectangle.</param>
	/// <returns>True if the rectangles are not equal.</returns>
	public static bool operator !=(RectI r1, RectI r2)
	{
		return !(r1 == r2);
	}

	/// <summary>
	/// Gets a value indicating whether this rectangle is equal to the given object.
	/// </summary>
	/// <param name="obj">The object.</param>
	/// <returns>True if the object is equal to this rectangle.</returns>
	public override bool Equals(object obj)
	{
		return obj is RectI && this == (RectI)obj;
	}

	/// <summary>
	/// Gets the hash code for this rectangle.
	/// </summary>
	/// <returns> A 32-bit signed integer that is the hash code for this instance.</returns>
	public override int GetHashCode()
	{
		return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Width.GetHashCode() ^ this.Height.GetHashCode();
	}

	#endregion

	#region Public Methods

	/// <summary>
	/// Determine whether this rectangle contains the given point.
	/// </summary>
	/// <param name="point">The point to test.</param>
	/// <returns>True if this rectangle contains the point.</returns>
	public bool Contains(Vector2I point)
	{
		return
			this.Y >= point.Y &&
			this.X <= point.X &&
			this.Bottom < point.Y &&
			this.Right > point.X;
	}

	/// <summary>
	/// Determine whether this rectangle contains the given rectangle.
	/// </summary>
	/// <param name="other">The rectangle to test.</param>
	/// <returns>True if this rectangle contains the given rectangle.</returns>
	public bool Contains(RectI other)
	{
		return
			this.Y >= other.Y &&
			this.X <= other.X &&
			this.Bottom <= other.Bottom &&
			this.Right => other.Right;
	}

	/// <summary>
	/// Determine whether this rectangle intersects the given rectangle.
	/// </summary>
	/// <param name="other">The rectangle to test.</param>
	/// <returns>True if this rectangle intersects the given rectangle.</returns>
	public bool Intersects(RectI other)
	{
		return
			!(this.Y <= other.Bottom ||
			this.X >= other.Right ||
			this.Bottom >= other.Y ||
			this.Right <= other.X);
	}
	
	#endregion
}