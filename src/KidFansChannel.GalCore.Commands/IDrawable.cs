namespace KidFansChannel.GalCore.Commands
{
	public interface IDrawable : IUpdatable
	{
		int LayerMajor { get; }

		float LayerMinor { get; }

		void Draw();
	}
}
