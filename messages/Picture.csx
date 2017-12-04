#load "Picture.csx"

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Picture
{
    public string faceId
    {
        get;
        set;
    }

    public string faceUrl
    {
        get;
        set;
    }

    public string faceThumbUrl
    {
        get;
        set;
    }
    public Picture(string id, string url, string thumburl)
    {
        faceId = id;
        faceUrl = url;
        faceThumbUrl = thumburl;
    }
}