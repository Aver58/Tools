#region Copyright © 2018 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    CommandPool.cs
 Author:      Zeng Zhiwei
 Time:        2019/11/7 10:05:30
=====================================================
*/
#endregion

//https://gameinstitute.qq.com/community/detail/128898
using System.Collections.Generic;

public class CommandPool
{
    #region fields
    private int maxCommandCount;
    private Stack<Command> m_redoStack;// 重做栈【后进先出】
    private Deque<Command> m_undoDeque;// 撤销栈【双端栈】
    #endregion

    #region properties
    public int TotalCommandCount
    {
        get
        {
            return m_undoDeque.Count + m_redoStack.Count;
        }
    }

    public int ToRedoCommandCount
    {
        get
        {
            return m_redoStack.Count;
        }
    }

    public int ToUndoCommandCount
    {
        get
        {
            return m_undoDeque.Count;
        }
    }

    public Stack<Command> RedoStack { get { return m_redoStack; } }
    public Deque<Command> UndoStack { get { return m_undoDeque; } }
    #endregion

    #region constructors
    public CommandPool()
    {
        m_undoDeque = new Deque<Command>();
        m_redoStack = new Stack<Command>();
        this.maxCommandCount = 1;
    }

    public CommandPool(int maxCommandCount)
    {
        m_undoDeque = new Deque<Command>(maxCommandCount);
        m_redoStack = new Stack<Command>();
        this.maxCommandCount = maxCommandCount;
    }
    #endregion

    #region methods
    public void Register(Command command)
    {
        m_redoStack.Clear();

        if (m_undoDeque.Count == maxCommandCount)
        {
            m_undoDeque.RemoveHead();
        }

        m_undoDeque.AddTail(command);
    }

    public void Undo()
    {
        if (m_undoDeque.Count == 0)
        {
            return;
        }

        Command command = m_undoDeque.RemoveTail();
        command.Undo();
        m_redoStack.Push(command);
    }

    public void Redo()
    {
        if (m_redoStack.Count == 0)
        {
            return;
        }

        Command command = m_redoStack.Pop();
        command.Excute();
        m_undoDeque.AddTail(command);
    }

    public Command GetNextUndoCommand()
    {
        Command command = m_undoDeque.GetTail();

        if (command == null)
            return null;

        return command;
    }

    public Command GetNextRedoCommand()
    {
        if (m_redoStack.Count == 0)
            return null;

        Command command = m_redoStack.Peek();
        return command;
    }

    public override string ToString()
    {
        return "this pool has " + m_redoStack.Count + " to redo command left and " + m_undoDeque.Count + " to undo command left";
    }
    #endregion
}

