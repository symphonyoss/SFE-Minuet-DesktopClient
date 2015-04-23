namespace Symphony.Mvvm
{
    public class EmptyArgs
    {
        private static readonly EmptyArgs Args = new EmptyArgs();

        public static EmptyArgs Empty
        {
            get { return Args; }
        }
    }
}
