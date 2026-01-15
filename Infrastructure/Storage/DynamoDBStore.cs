//using Amazon.DynamoDBv2;
//using Amazon.DynamoDBv2.Model;

//public static class DynamoDBStore
//{
//    public static async Task CreateTodoTableAsync(IAmazonDynamoDB client)
//    {
//        if (client == null)
//            throw new ArgumentNullException(nameof(client));
//        try
//        {
//            await client.DescribeTableAsync("TodoTasks");
//            return; 
//        }
//        catch (ResourceNotFoundException)
//        {
//        }
//        var request = new CreateTableRequest
//        {
//            TableName = "TodoTasks",
//            AttributeDefinitions = new List<AttributeDefinition>
//            {
//                new AttributeDefinition
//                {
//                    AttributeName = "UserId",
//                    AttributeType = ScalarAttributeType.S
//                },
//                new AttributeDefinition
//                {
//                    AttributeName = "TaskId",
//                    AttributeType = ScalarAttributeType.S
//                }
//            },
//            KeySchema = new List<KeySchemaElement>
//            {
//                new KeySchemaElement
//                {
//                    AttributeName = "UserId",
//                    KeyType = KeyType.HASH
//                },
//                new KeySchemaElement
//                {
//                    AttributeName = "TaskId",
//                    KeyType = KeyType.RANGE
//                }
//            },
//            ProvisionedThroughput = new ProvisionedThroughput
//            {
//                ReadCapacityUnits = 5,
//                WriteCapacityUnits = 5
//            }
//        };
//        await client.CreateTableAsync(request);
//    }
//}
