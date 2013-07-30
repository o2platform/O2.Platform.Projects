using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentSharp.WinForms;
using FluentSharp.CassiniDev;
using FluentSharp.CoreLib;
using NUnit.Framework;
using Website.O2Platform.Controllers;

namespace UnitTests.Website_O2Platform
{
    [TestFixture]
    public class Test_Mvc_Setup
    {
        public string webRoot;

        public Test_Mvc_Setup()
        {
            var controllerAssembly = typeof(TestController).assembly_Location();
            webRoot = controllerAssembly.parentFolder().parentFolder();    
            Assert.IsTrue(webRoot.dirExists());            
            Assert.IsTrue(webRoot.pathCombine("Web.config").fileExists());
            Assert.IsTrue(webRoot.pathCombine("bin").pathCombine(controllerAssembly.fileName()).fileExists());            
        }

        [Test]
        public void Get_Controller_Via_Cassini()
        {                               
            var server = new API_Cassini(webRoot);

            server.start();

            var testUrl     = server.url() + "Test";
            var html        = testUrl.html();
            var expected    = "Razor Page Test";
            
            
            Assert.IsTrue(html.contains(expected));

            /*"view site".popupWindow()
                       .add_WebBrowser_with_NavigationBar()
                       .open(server.url())
                       .waitForClose();
            */
            server.stop();
        }
    }
}
