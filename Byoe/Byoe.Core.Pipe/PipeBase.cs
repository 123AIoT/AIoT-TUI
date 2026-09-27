using System;

namespace Byoe.Core.Pipe
{
	/// <summary>
	/// 管道的基础类对象
	/// </summary>
	public class PipeBase : IDisposable
	{
		private SimpleHybirdLock hybirdLock;

		/// <inheritdoc cref="P:Byoe.Core.SimpleHybirdLock.LockingTick" />
		public int LockingTick => hybirdLock.LockingTick;

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public PipeBase()
		{
			hybirdLock = new SimpleHybirdLock();
		}

		/// <inheritdoc cref="M:Byoe.Core.SimpleHybirdLock.Enter" />
		public bool PipeLockEnter()
		{
			return hybirdLock.Enter();
		}

		/// <inheritdoc cref="M:Byoe.Core.SimpleHybirdLock.Leave" />
		public bool PipeLockLeave()
		{
			return hybirdLock.Leave();
		}

		/// <inheritdoc cref="M:System.IDisposable.Dispose" />
		public virtual void Dispose()
		{
			hybirdLock?.Dispose();
		}
	}
}
