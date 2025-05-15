public class PseudoSequence
{
    public int SequenceID { get; }
    public int IndexFirstItem { get; }

    public PseudoSequence(int sequenceId, int indexFirstItem)
    {
        this.SequenceID = sequenceId;
        this.IndexFirstItem = indexFirstItem;
    }
}