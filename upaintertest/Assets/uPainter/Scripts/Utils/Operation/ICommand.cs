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
    /// 具体逻辑单元，用于支持撤销与重做
    /// </summary>
	public interface ICommand
    {
        int SID { get; }
        object Target { get; }
        string StateName { get; set; }

        string GetUniqueName();

        void Do();

		void Undo();

		void Destroy();

	}
}

