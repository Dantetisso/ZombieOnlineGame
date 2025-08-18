using UnityEngine;
using System;

public class QuestionNode : ITreeNode
{
    Func<bool> _question;
    ITreeNode _tNode;
    ITreeNode _fNode;

    public QuestionNode(Func<bool> question, ITreeNode tNode, ITreeNode fNode)
    {
        _question = question;
        _tNode = tNode;
        _fNode = fNode;
    }
    public void Execute()
    {
        if (_question())
        {
            _tNode.Execute();
        }
        else
        {
            _fNode.Execute();
        }
    }
}
