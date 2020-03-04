using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using OrchardCore.ContentManagement;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers
{
    public class DeleteGraphSyncer : IDeleteGraphSyncer
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IDeleteNodeCommand _deleteNodeCommand;
        private readonly ISession _session;

        public DeleteGraphSyncer(IGraphDatabase graphDatabase, IDeleteNodeCommand deleteNodeCommand, ISession session)
        {
            _graphDatabase = graphDatabase;
            _deleteNodeCommand = deleteNodeCommand;
            _session = session;
        }

        public async Task DeleteFromGraph(ContentItem contentItem)
        {
            if (contentItem.Content.GraphSyncPart == null)
                return;

            _deleteNodeCommand.ContentType = contentItem.ContentType;
            _deleteNodeCommand.Uri = contentItem.Content.GraphSyncPart.Text;

            try
            {
                await _graphDatabase.RunWriteCommands(_deleteNodeCommand);
            }
            //TODO : specify which exceptions to handle?
            catch
            {
                //this forces a rollback of the currect OC db transaction
                _session.Cancel();
                throw;
            }
        }
    }
}
