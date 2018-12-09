namespace ViretTool.BusinessLayer.Services
{
    public interface IDatabaseServicesFactory
    {
        DatasetServices Create(string datasetFolder);


        void Release(DatasetServices databaseServices);
    }
}
