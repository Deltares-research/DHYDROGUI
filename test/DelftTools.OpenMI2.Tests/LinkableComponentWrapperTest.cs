using NUnit.Framework;
using OpenMI.Standard2;

namespace DelftTools.OpenMI2.Tests
{
    [TestFixture]
    public class LinkableComponentWrapperTest
    {
        [Test]
        public void SuccesFullyRunModelUsingLinkableComponentWrapper()
        {
            var linkableComponent = new LinkableComponentWrapper(new SimpleModel());

            //start out created
            Assert.AreEqual(LinkableComponentStatus.Created,linkableComponent.Status);
            
            //initialize gets us to initialized (via initializing?)
            linkableComponent.Initialize();
            Assert.AreEqual(LinkableComponentStatus.Initialized,linkableComponent.Status);
            
            //validate gets us to valid (via validating?)
            linkableComponent.Validate();
            Assert.AreEqual(LinkableComponentStatus.Valid, linkableComponent.Status);
            
            //prepare gets us to updated (via preparing?)
            linkableComponent.Prepare();
            Assert.AreEqual(LinkableComponentStatus.Updated,linkableComponent.Status);

            //update gets us to done..or updated 
            while (linkableComponent.Status == LinkableComponentStatus.Updated)
            {
                linkableComponent.Update();
            }

            //we should be done..
            Assert.AreEqual(LinkableComponentStatus.Done,linkableComponent.Status);

            //done should finish and get finished
            linkableComponent.Finish();
            Assert.AreEqual(LinkableComponentStatus.Finished,linkableComponent.Status);
        }
    }
}
