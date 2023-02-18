using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AwsImageGallery
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            // TAG Stack
            var app = new App();
            _ = new AwsImageGalleryStack(app, "AwsImageGalleryStack");
            app.Synth();
        }
    }
}
