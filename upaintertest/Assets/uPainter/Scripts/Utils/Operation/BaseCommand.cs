namespace Wing.uPainter
{
	public class BaseCommand : ICommand
	{
        static int _sid = 0;
        int _id = ++_sid;

        public virtual string StateName { get; set; }

        public virtual object Target
        {
            get
            {
                return null;
            }
        }

        public int SID
        {
            get
            {
                return _id;
            }
        }

        public virtual void Do()
        {
        }

        public virtual void Undo()
        {
        }

        public virtual void Destroy()
        {
            
        }

        public virtual string GetUniqueName()
        {
            return GetType().Name;
        }
    }
}

