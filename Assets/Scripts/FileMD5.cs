using System.Collections;
using System.Collections.Generic;

public class FileMD5Info
{
    public string fileName;
    public string fileMd5;

    public FileMD5Info()
    {
            
    }
}

public class FileMD5
{
    public string versions;
    public List<FileMD5Info> fileMD5Infos = new List<FileMD5Info>();

    public FileMD5()
    {
        
    }
}