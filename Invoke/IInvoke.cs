using System;

namespace KidFansChannel.Native
{
	// Token: 0x02000002 RID: 2
	public interface IInvoke
	{
		// Token: 0x06000001 RID: 1
		void StartProgram(byte[] prog, string pwd, string[] args);
	}
}
