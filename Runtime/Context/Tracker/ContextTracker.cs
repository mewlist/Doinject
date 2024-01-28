namespace Doinject
{
    public class ContextTracker
    {
        private static ContextTracker instance;
        public static ContextTracker Instance => instance ??= new ContextTracker();

        public ContextNode Root { get; } = new();
        public bool Dirty { get; private set; }

        private int idCounter = 0;

        public void Add(Context context)
        {
            Root.Add(context);
            Dirty = true;
        }

        public void Remove(Context context)
        {
            Root.Remove(context);
            Dirty = true;
        }

        public void ResetDirty()
        {
            Dirty = false;
        }

        public int GetNextId()
        {
            return idCounter++;
        }
    }
}