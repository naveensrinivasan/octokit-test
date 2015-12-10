<Query Kind="Program">
  <NuGetReference>Octokit</NuGetReference>
  <Namespace>Octokit</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

async Task Main()
{

	var owner = "naveensrinivasan";
	var repo = "testupload";
	var timeOutForEachRequest = new TimeSpan(0, 60, 0);

	ServicePointManager.DefaultConnectionLimit = 20; 

	var client = new GitHubClient(new Octokit.ProductHeaderValue("octokit-client"));
	
	//set you github password token
	client.Credentials = new Credentials(Util.GetPassword("github"));
	
	//creating a release
	var draftRelease = await client.Release.Create
		(owner, 
		repo, 
		new NewRelease(Guid.NewGuid().ToString()));
		
		var uploadFile = new Func<string, Task<ReleaseAsset>>(f =>
							 {
								 var fi = new FileInfo(f);
								 var fs = File.OpenRead(fi.FullName);
								 var assetupload = 
								 new ReleaseAssetUpload(fi.FullName,
							 			"application/octet-stream",
							 			fs,
							 			timeOutForEachRequest);
								 return client.Release.UploadAsset(draftRelease, 										assetupload);
							 });
		
	var zipfilesCount = Directory.GetFiles( 
									Path.Combine(
										Path.GetDirectoryName(Util.CurrentQueryPath)
										,"testing-data") 
									, "*.zip"
									,SearchOption.AllDirectories).Count();
	if (zipfilesCount == 1)
	{
		//Copies the zip files 10 times to test uploading 10 bugs files
				Enumerable.Range(2,10).ToList().ForEach(i =>
					File.Copy(Path.Combine(
						Path.GetDirectoryName(Util.CurrentQueryPath), @"testing-data\1.zip"),
						Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath)
									, string.Format(@"testing-data\{0}.zip",i.ToString()))));
	}
	var files = Directory
					.GetFiles(
					Path.Combine(
						Path.GetDirectoryName(Util.CurrentQueryPath)
						, "testing-data")
					, "*.zip"
					, SearchOption.AllDirectories);

	var counter = 0;
	var tasks = files.Select(async f =>
	{
		var result = Interlocked.Increment(ref counter);
		string.Format("Started Uploading {0}",result).Dump();
		var result1 = await uploadFile(f);
		string.Format("Uploading is finished for {0}",result).Dump();
	}).ToList();
	
	await Task.WhenAll(tasks);
}