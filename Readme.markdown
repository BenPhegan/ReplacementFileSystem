A quick and simple copy/paste of some code that was found here:

[CodePlexClient](http://codeplexclient.codeplex.com/)

As per the blog below, I believe the attribution should go to Brad Wilson.  I just packaged it up for simplicity.  I am sure any resulting errors are mine, not his (unless they are his, of course).

[Ade Miller](http://www.ademiller.com/blogs/tech/2007/12/mocking-the-file-system/)

Provides the following:

* An IFileSystem definition

* A FileSystem implementation that devolves most of the work directly to low level C# System.IO calls.

* A MockFileSystem implementation that provides a fairly complete mock filesystem implementation that tracks modification to simplify testing.

This was mainly put together to simplify and standardise the number and type of IFileSystem implementations that I was using, writing or seeing.