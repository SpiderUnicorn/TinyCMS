using System;
using System.Collections.Generic;
using System.ComponentModel;
using TinyCMS.Data;

namespace TinyCMS.QuestionNodes
{
    [Serializable]
    public class QuestionAnswer : INotifyPropertyChanged
    {
        public string Val { get; set; }
        public int Points { get; set; }

#pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore
    }

    [Serializable]
    public class QuestionCategory : BaseNode
    {
        public override string Type => "form-category";

        public string Title { get; set; }
    }

    [Serializable]
    public class Question : BaseNode
    {
        public override string Type => "question";

        public string Title { get; set; }

        public List<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    }

    [Serializable]
    public class QuestionMultiple : BaseNode
    {
        public override string Type => "question-multiple";

        public string Title { get; set; }
        public List<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    }
}
