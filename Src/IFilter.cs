namespace Photoshop.Src;

public interface IFilter
{
    public double[] Parameters {get;}
    public Picture Modify(IReadOnlyPicture originalPicture, double[] parameters);
}
