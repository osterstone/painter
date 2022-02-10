﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Wing.uPainter
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

    /// <summary>
    /// 操作栈
    /// </summary>
	public interface IOperation 
	{
		int MaxStepNumber { get;set; }

        event Action<ICommand> OnUndo;
        event Action<ICommand> OnRedo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="reallyDo">是否需要调用一次redo</param>
		void DoCommand(ICommand cmd, bool reallyDo = true);

		bool HasUndo();

		bool HasRedo();

        ICommand Undo();

        ICommand Redo();

        void DoCommand(Action doCmd, Action undoCmd);

        bool IsDirty();

        void ClearDirtyFlag();
    }
}

