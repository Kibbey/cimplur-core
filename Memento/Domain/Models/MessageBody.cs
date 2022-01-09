using System.Collections.Generic;

namespace Domain.Models
{
    public class MessageBody
    {
        public string Message { get; set; }
    }

    public class Message
    {
        public string state { get; set; }
        public string version { get; set; }
        public string jobId { get; set; }
        public Input input { get; set; }
        public List<Output> outputs { get; set; }
    }
    
    public class Input {
        public string key { get;  set;}
    }

    public class Output { 
        public string key { get; set; }
    }
}

//{
//  "state" : "COMPLETED",
//  "version" : "2012-09-25",
//  "jobId" : "1478574570158-t6i6ud",
//  "pipelineId" : "1477544179677-j0ddby",
//  "input" : {
//    "key" : "temp/test/1/3089/m/1039"
//  },
//  "outputs" : [ {
//    "id" : "1",
//    "presetId" : "1351620000001-000050",
//    "key" : "test/1/3089/m/1039",
//    "thumbnailPattern" : "test_1_3089_m_1039_{count}",
//    "status" : "Complete",
//    "duration" : 5,
//    "width" : 202,
//    "height" : 360
//  } ]
//}
