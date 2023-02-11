using System.Reflection;

using cc.isr.VXI11.Logging;

using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
namespace cc.isr.VXI11.MSTest
{
    [TestClass]
    public class IdentityParserTests
    {


        private readonly string[] _identities = { "INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434",
                                                  "INTEGRATED SCIENTIFIC RESOURCES,MODEL IEEE488Mock,001,1.0.8434" };

        /// <summary>   Assert identity parse. </summary>
        /// <remarks>   2023-02-11. </remarks>
        /// <param name="identity"> The identity. </param>
        private static void AssertIdentityParse( string identity )
        {
            cc.isr.VXI11.IdentityParser parser = new( identity );
            string builtIdentity = parser.BuildIdentity();
            Assert.AreEqual( identity, builtIdentity, $"Identity {builtIdentity} built from parsed {identity} not matching" );
            Logger.Writer.LogInformation( $"Identity is {(string.IsNullOrEmpty( parser.Identity ) ? "empty" : parser.Identity)} for {identity} " );
        }

        /// <summary>   (Unit Test Method) identity parse. </summary>
        /// <remarks>
        /// 2023-02-11.
        /// <code>
        /// </code>
        /// </remarks>
        [TestMethod]
        public void IdentityParse()
        {
            foreach ( string Identity in this._identities )
            {
                AssertIdentityParse( Identity );
            }
        }




    }
}
