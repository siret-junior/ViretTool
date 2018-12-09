namespace ViretTool.BusinessLayer.Services
{
    public interface IDatabaseServicesFactory
    {
        DatasetServices Create(string datasetDirectory);


        void Release(DatasetServices databaseServices);
    }
}
