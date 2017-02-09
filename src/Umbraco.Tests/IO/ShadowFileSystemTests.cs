﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.IO
{
    [TestFixture]
    public class ShadowFileSystemTests
    {
        // tested:
        // only 1 instance of this class is created
        // SetUp and TearDown run before/after each test
        // SetUp does not start before the previous TearDown returns

        [SetUp]
        public void SetUp()
        {
            SafeCallContext.Clear();
            ClearFiles();
            ShadowFileSystems.ResetId();
        }

        [TearDown]
        public void TearDown()
        {
            SafeCallContext.Clear();
            ClearFiles();
            ShadowFileSystems.ResetId();
        }

        private static void ClearFiles()
        {
            TestHelper.DeleteDirectory(IOHelper.MapPath("FileSysTests"));
            TestHelper.DeleteDirectory(IOHelper.MapPath("App_Data"));
        }

        private static string NormPath(string path)
        {
            return path.ToLowerInvariant().Replace("\\", "/");
        }

        [Test]
        public void ShadowDeleteDirectory()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/d1");
            Directory.CreateDirectory(path + "/ShadowTests/d2");

            var files = fs.GetFiles("");
            Assert.AreEqual(0, files.Count());

            var dirs = fs.GetDirectories("");
            Assert.AreEqual(2, dirs.Count());
            Assert.IsTrue(dirs.Contains("d1"));
            Assert.IsTrue(dirs.Contains("d2"));

            ss.DeleteDirectory("d1");

            Assert.IsTrue(Directory.Exists(path + "/ShadowTests/d1"));
            Assert.IsTrue(fs.DirectoryExists("d1"));
            Assert.IsFalse(ss.DirectoryExists("d1"));

            dirs = ss.GetDirectories("");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("d2"));
        }

        [Test]
        public void ShadowDeleteDirectoryInDir()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/sub");
            Directory.CreateDirectory(path + "/ShadowTests/sub/d1");
            Directory.CreateDirectory(path + "/ShadowTests/sub/d2");

            var files = fs.GetFiles("");
            Assert.AreEqual(0, files.Count());

            var dirs = ss.GetDirectories("");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub"));

            dirs = fs.GetDirectories("sub");
            Assert.AreEqual(2, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub/d1"));
            Assert.IsTrue(dirs.Contains("sub/d2"));

            dirs = ss.GetDirectories("sub");
            Assert.AreEqual(2, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub/d1"));
            Assert.IsTrue(dirs.Contains("sub/d2"));

            ss.DeleteDirectory("sub/d1");

            Assert.IsTrue(Directory.Exists(path + "/ShadowTests/sub/d1"));
            Assert.IsTrue(fs.DirectoryExists("sub/d1"));
            Assert.IsFalse(ss.DirectoryExists("sub/d1"));

            dirs = fs.GetDirectories("sub");
            Assert.AreEqual(2, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub/d1"));
            Assert.IsTrue(dirs.Contains("sub/d2"));

            dirs = ss.GetDirectories("sub");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub/d2"));
        }

        [Test]
        public void ShadowDeleteFile()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            File.WriteAllText(path + "/ShadowTests/f1.txt", "foo");
            File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

            var files = fs.GetFiles("");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("f1.txt"));
            Assert.IsTrue(files.Contains("f2.txt"));

            files = ss.GetFiles("");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("f1.txt"));
            Assert.IsTrue(files.Contains("f2.txt"));

            var dirs = ss.GetDirectories("");
            Assert.AreEqual(0, dirs.Count());

            ss.DeleteFile("f1.txt");

            Assert.IsTrue(File.Exists(path + "/ShadowTests/f1.txt"));
            Assert.IsTrue(fs.FileExists("f1.txt"));
            Assert.IsFalse(ss.FileExists("f1.txt"));

            files = ss.GetFiles("");
            Assert.AreEqual(1, files.Count());
            Assert.IsTrue(files.Contains("f2.txt"));
        }

        [Test]
        public void ShadowDeleteFileInDir()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/sub");
            File.WriteAllText(path + "/ShadowTests/sub/f1.txt", "foo");
            File.WriteAllText(path + "/ShadowTests/sub/f2.txt", "foo");

            var files = fs.GetFiles("");
            Assert.AreEqual(0, files.Count());

            files = fs.GetFiles("sub");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("sub/f1.txt"));
            Assert.IsTrue(files.Contains("sub/f2.txt"));

            files = ss.GetFiles("");
            Assert.AreEqual(0, files.Count());

            var dirs = ss.GetDirectories("");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub"));

            files = ss.GetFiles("sub");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("sub/f1.txt"));
            Assert.IsTrue(files.Contains("sub/f2.txt"));

            dirs = ss.GetDirectories("sub");
            Assert.AreEqual(0, dirs.Count());

            ss.DeleteFile("sub/f1.txt");

            Assert.IsTrue(File.Exists(path + "/ShadowTests/sub/f1.txt"));
            Assert.IsTrue(fs.FileExists("sub/f1.txt"));
            Assert.IsFalse(ss.FileExists("sub/f1.txt"));

            files = fs.GetFiles("sub");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("sub/f1.txt"));
            Assert.IsTrue(files.Contains("sub/f2.txt"));

            files = ss.GetFiles("sub");
            Assert.AreEqual(1, files.Count());
            Assert.IsTrue(files.Contains("sub/f2.txt"));
        }

        [Test]
        public void ShadowCantCreateFile()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Assert.Throws<FileSecurityException>(() =>
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                    ss.AddFile("../../f1.txt", ms);
            });
        }

        [Test]
        public void ShadowCreateFile()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("f1.txt", ms);

            Assert.IsTrue(File.Exists(path + "/ShadowTests/f2.txt"));
            Assert.IsFalse(File.Exists(path + "/ShadowSystem/f2.txt"));
            Assert.IsTrue(fs.FileExists("f2.txt"));
            Assert.IsTrue(ss.FileExists("f2.txt"));

            Assert.IsFalse(File.Exists(path + "/ShadowTests/f1.txt"));
            Assert.IsTrue(File.Exists(path + "/ShadowSystem/f1.txt"));
            Assert.IsFalse(fs.FileExists("f1.txt"));
            Assert.IsTrue(ss.FileExists("f1.txt"));

            var files = ss.GetFiles("");
            Assert.AreEqual(2, files.Count());
            Assert.IsTrue(files.Contains("f1.txt"));
            Assert.IsTrue(files.Contains("f2.txt"));

            string content;
            using (var stream = ss.OpenFile("f1.txt"))
                content = new StreamReader(stream).ReadToEnd();

            Assert.AreEqual("foo", content);
        }

        [Test]
        public void ShadowCreateFileInDir()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("sub/f1.txt", ms);

            Assert.IsFalse(File.Exists(path + "/ShadowTests/sub/f1.txt"));
            Assert.IsTrue(File.Exists(path + "/ShadowSystem/sub/f1.txt"));
            Assert.IsFalse(fs.FileExists("sub/f1.txt"));
            Assert.IsTrue(ss.FileExists("sub/f1.txt"));

            Assert.IsFalse(fs.DirectoryExists("sub"));
            Assert.IsTrue(ss.DirectoryExists("sub"));

            var dirs = fs.GetDirectories("");
            Assert.AreEqual(0, dirs.Count());

            dirs = ss.GetDirectories("");
            Assert.AreEqual(1, dirs.Count());
            Assert.IsTrue(dirs.Contains("sub"));

            var files = ss.GetFiles("sub");
            Assert.AreEqual(1, files.Count());

            string content;
            using (var stream = ss.OpenFile("sub/f1.txt"))
                content = new StreamReader(stream).ReadToEnd();

            Assert.AreEqual("foo", content);
        }

        [Test]
        public void ShadowAbort()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("path/to/some/dir/f1.txt", ms);

            Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"));

            // let the shadow fs die
        }

        [Test]
        public void ShadowComplete()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            Directory.CreateDirectory(path + "/ShadowTests/sub/sub");
            File.WriteAllText(path + "/ShadowTests/sub/sub/f2.txt", "foo");

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("path/to/some/dir/f1.txt", ms);
            ss.DeleteFile("sub/sub/f2.txt");

            Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt"));

            ss.Complete();

            Assert.IsTrue(File.Exists(path + "/ShadowSystem/path/to/some/dir/f1.txt")); // *not* cleaning
            Assert.IsTrue(File.Exists(path + "/ShadowTests/path/to/some/dir/f1.txt"));
            Assert.IsFalse(File.Exists(path + "/ShadowTests/sub/sub/f2.txt"));
        }

        [Test]
        public void ShadowScopeComplete()
        {
            var path = IOHelper.MapPath("FileSysTests");
            var appdata = IOHelper.MapPath("App_Data");
            Directory.CreateDirectory(path);

            var scopedFileSystems = false;
            var scopeProvider = MockScopeProvider(() => scopedFileSystems);

            var fs = new PhysicalFileSystem(path, "ignore");
            var sw = new ShadowWrapper(fs, "shadow", scopeProvider);
            var swa = new[] {sw};

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f1.txt", ms);
            Assert.IsTrue(fs.FileExists("sub/f1.txt"));

            Guid id;

            // explicit shadow without scope does not work
            sw.Shadow(id = Guid.NewGuid());
            Assert.IsTrue(Directory.Exists(appdata + "/Shadow/" + id));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f2.txt", ms);
            Assert.IsTrue(fs.FileExists("sub/f2.txt"));
            sw.UnShadow(true);
            Assert.IsTrue(fs.FileExists("sub/f2.txt"));
            Assert.IsFalse(Directory.Exists(appdata + "/Shadow/" + id));

            // shadow with scope but no complete does not complete
            scopedFileSystems = true; // pretend we have a scope
            var scope = new ShadowFileSystems(id = Guid.NewGuid(), swa);
            Assert.IsTrue(Directory.Exists(appdata + "/Shadow/" + id));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f3.txt", ms);
            Assert.IsFalse(fs.FileExists("sub/f3.txt"));
            Assert.AreEqual(1, Directory.GetDirectories(appdata + "/Shadow").Length);
            scope.Dispose();
            scopedFileSystems = false;
            Assert.IsFalse(fs.FileExists("sub/f3.txt"));
            Assert.IsFalse(Directory.Exists(appdata + "/Shadow/" + id));

            // shadow with scope and complete does complete
            scopedFileSystems = true; // pretend we have a scope
            scope = new ShadowFileSystems(id = Guid.NewGuid(), swa);
            Assert.IsTrue(Directory.Exists(appdata + "/Shadow/" + id));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f4.txt", ms);
            Assert.IsFalse(fs.FileExists("sub/f4.txt"));
            Assert.AreEqual(1, Directory.GetDirectories(appdata + "/Shadow").Length);
            scope.Complete();
            scope.Dispose();
            scopedFileSystems = false;
            TestHelper.TryAssert(() => Assert.AreEqual(0, Directory.GetDirectories(appdata + "/Shadow").Length));
            Assert.IsTrue(fs.FileExists("sub/f4.txt"));
            Assert.IsFalse(Directory.Exists(appdata + "/Shadow/" + id));

            // test scope for "another thread"

            scopedFileSystems = true; // pretend we have a scope
            scope = new ShadowFileSystems(id = Guid.NewGuid(), swa);
            Assert.IsTrue(Directory.Exists(appdata + "/Shadow/" + id));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f5.txt", ms);
            Assert.IsFalse(fs.FileExists("sub/f5.txt"));

            // pretend we're another thread w/out scope
            scopedFileSystems = false;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f6.txt", ms);
            scopedFileSystems = true; // pretend we have a scope

            Assert.IsTrue(fs.FileExists("sub/f6.txt")); // other thread has written out to fs
            scope.Complete();
            scope.Dispose();
            scopedFileSystems = false;
            Assert.IsTrue(fs.FileExists("sub/f5.txt"));
            Assert.IsFalse(Directory.Exists(appdata + "/Shadow/" + id));
        }

        [Test]
        public void ShadowScopeCompleteWithFileConflict()
        {
            var path = IOHelper.MapPath("FileSysTests");
            var appdata = IOHelper.MapPath("App_Data");
            Directory.CreateDirectory(path);

            var scopedFileSystems = false;
            var scopeProvider = MockScopeProvider(() => scopedFileSystems);

            var fs = new PhysicalFileSystem(path, "ignore");
            var sw = new ShadowWrapper(fs, "shadow", scopeProvider);
            var swa = new[] {sw};

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f1.txt", ms);
            Assert.IsTrue(fs.FileExists("sub/f1.txt"));

            Guid id;

            scopedFileSystems = true; // pretend we have a scope
            var scope = new ShadowFileSystems(id = Guid.NewGuid(), swa);
            Assert.IsTrue(Directory.Exists(appdata + "/Shadow/" + id));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f2.txt", ms);
            Assert.IsFalse(fs.FileExists("sub/f2.txt"));

            // pretend we're another thread w/out scope
            scopedFileSystems = false;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar")))
                sw.AddFile("sub/f2.txt", ms);
            scopedFileSystems = true; // pretend we have a scope

            Assert.IsTrue(fs.FileExists("sub/f2.txt")); // other thread has written out to fs
            scope.Complete();
            scope.Dispose();
            scopedFileSystems = false;
            Assert.IsTrue(fs.FileExists("sub/f2.txt"));
            TestHelper.TryAssert(() => Assert.IsFalse(Directory.Exists(appdata + "/Shadow/" + id)));

            string text;
            using (var s = fs.OpenFile("sub/f2.txt"))
            using (var r = new StreamReader(s))
                text = r.ReadToEnd();

            // the shadow filesystem will happily overwrite anything it can
            Assert.AreEqual("foo", text);
        }

        [Test]
        public void ShadowScopeCompleteWithDirectoryConflict()
        {
            var path = IOHelper.MapPath("FileSysTests");
            var appdata = IOHelper.MapPath("App_Data");
            Directory.CreateDirectory(path);

            var scopedFileSystems = false;
            var scopeProvider = MockScopeProvider(() => scopedFileSystems);

            var fs = new PhysicalFileSystem(path, "ignore");
            var sw = new ShadowWrapper(fs, "shadow", scopeProvider);
            var swa = new[] {sw};

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f1.txt", ms);
            Assert.IsTrue(fs.FileExists("sub/f1.txt"));

            Guid id;

            scopedFileSystems = true; // pretend we have a scope
            var scope = new ShadowFileSystems(id = Guid.NewGuid(), swa);
            Assert.IsTrue(Directory.Exists(appdata + "/Shadow/" + id));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f2.txt", ms);
            Assert.IsFalse(fs.FileExists("sub/f2.txt"));

            // pretend we're another thread w/out scope
            scopedFileSystems = false;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("bar")))
                sw.AddFile("sub/f2.txt/f2.txt", ms);
            scopedFileSystems = true; // pretend we have a scope

            Assert.IsTrue(fs.FileExists("sub/f2.txt/f2.txt")); // other thread has written out to fs

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                sw.AddFile("sub/f3.txt", ms);
            Assert.IsFalse(fs.FileExists("sub/f3.txt"));

            scope.Complete();

            try
            {
                // no way this can work since we're trying to write a file
                // but there's now a directory with the same name on the real fs
                scope.Dispose();
                Assert.Fail("Expected AggregateException.");
            }
            catch (AggregateException ae)
            {
                Assert.AreEqual(1, ae.InnerExceptions.Count);
                var e = ae.InnerExceptions[0];
                Assert.IsNotNull(e.InnerException);
                Assert.IsInstanceOf<AggregateException>(e);
                ae = (AggregateException) e;

                Assert.AreEqual(1, ae.InnerExceptions.Count);
                e = ae.InnerExceptions[0];
                Assert.IsNotNull(e.InnerException);
                Assert.IsInstanceOf<UnauthorizedAccessException>(e.InnerException);
            }

            // still, the rest of the changes has been applied ok
            Assert.IsTrue(fs.FileExists("sub/f3.txt"));
        }

        [Test]
        public void GetFilesReturnsChildrenOnly()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            File.WriteAllText(path + "/f1.txt", "foo");
            Directory.CreateDirectory(path + "/test");
            File.WriteAllText(path + "/test/f2.txt", "foo");
            Directory.CreateDirectory(path + "/test/inner");
            File.WriteAllText(path + "/test/inner/f3.txt", "foo");

            path = NormPath(path);
            var files = Directory.GetFiles(path);
            Assert.AreEqual(1, files.Length);
            files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            Assert.AreEqual(3, files.Length);
            var efiles = Directory.EnumerateFiles(path);
            Assert.AreEqual(1, efiles.Count());
            efiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
            Assert.AreEqual(3, efiles.Count());
        }

        [Test]
        public void DeleteDirectoryAndFiles()
        {
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            File.WriteAllText(path + "/f1.txt", "foo");
            Directory.CreateDirectory(path + "/test");
            File.WriteAllText(path + "/test/f2.txt", "foo");
            Directory.CreateDirectory(path + "/test/inner");
            File.WriteAllText(path + "/test/inner/f3.txt", "foo");

            path = NormPath(path);
            TestHelper.Try(() => Directory.Delete(path, true));
            TestHelper.TryAssert(() => Assert.IsFalse(File.Exists(path + "/test/inner/f3.txt")));
        }

        [Test]
        public void MockTest()
        {
            var scoped = false;
            var provider = MockScopeProvider(() => scoped);

            Assert.IsFalse(provider.AmbientScope.ScopedFileSystems);
            scoped = true;
            Assert.IsTrue(provider.AmbientScope.ScopedFileSystems);
        }

        private static IScopeProviderInternal MockScopeProvider(Func<bool> f)
        {
            var scopeMock = new Mock<IScopeInternal>();
            scopeMock.Setup(x => x.ScopedFileSystems).Returns(f);
            var providerMock = new Mock<IScopeProviderInternal>();
            providerMock.Setup(x => x.AmbientScope).Returns(scopeMock.Object);
            return providerMock.Object;
        }

        [Test]
        public void ShadowGetFiles()
        {
            // Arrange
            var path = IOHelper.MapPath("FileSysTests");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(path + "/ShadowTests");
            Directory.CreateDirectory(path + "/ShadowSystem");

            var fs = new PhysicalFileSystem(path + "/ShadowTests/", "ignore");
            var sfs = new PhysicalFileSystem(path + "/ShadowSystem/", "ignore");
            var ss = new ShadowFileSystem(fs, sfs);

            // Act
            File.WriteAllText(path + "/ShadowTests/f2.txt", "foo");
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("foo")))
                ss.AddFile("f1.txt", ms);

            // Assert
            var getFiles = ss.GetFiles(string.Empty);
            Assert.AreEqual(2, getFiles.Count());
            var getFilesWithFilter = ss.GetFiles(string.Empty, "*");
            Assert.AreEqual(2, getFilesWithFilter.Count());
        }
    }
}
