namespace DocumentStore
{
    public interface ISerialisation
    {
        string Serialise(object document);
    }
}
