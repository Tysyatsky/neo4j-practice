using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using System.IO;
using System.Xml.Linq;

public class Graph : IDisposable
{
    private bool _disposed = false;
    private readonly IDriver _driver;
    private bool _created = false;

    public bool Created { get { return _created; } set { _created = value; } }
    ~Graph() => Dispose(false);

    public Graph(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    public void CreateGraph()
    {
        using (var session = _driver.Session())
        {
            for (int i = 1; i <= 7; i++)
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (a:Anime) " +
                                        $"SET a.name = 'Anime {i}' " +
                                        "RETURN 'Created ' + a.name + ', node id ' + id(a)"
                        );
                    return result.Single()[0].As<string>();
                });
                Console.WriteLine(greeting);
            }
            for (int i = 1; i <= 5; i++)
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (a:VoiceA) " +
                                        $"SET a.name = 'VoiceA {i}' " +
                                        "RETURN 'Created ' + a.name + ', node id ' + id(a)"
                        );
                    return result.Single()[0].As<string>();
                });
                Console.WriteLine(greeting);
            }
        }
    }

    public void CreateRelation(int number1, int number2, string name1, string name2)
    {
        using (var session = _driver.Session())
        {
            var creation = session.WriteTransaction(tx =>
            {
                var result = tx.Run($"MATCH (a:{name1}),(t:{name2}) " +
                                    $"WHERE a.name = '{name1} {number1}' AND t.name = '{name2} {number2}'" +
                                    "create (a)-[r:connected]->(t)" +
                                    "RETURN 'Connected ' + a.name + ' with '+ t.name"
                    );
                return result.Single()[0].As<string>();
            });
            Console.WriteLine(creation);
        }

    }

    public Dictionary<string, int> TestLength()
    {   
        Dictionary<string, int> result = new Dictionary<string, int>();

        using (var session = _driver.Session())
        {
            for (int i = 1; i < 5; i++)
            {
                for (int j = i + 1; j <= 5; j++)
                {
                    var creation = session.WriteTransaction(tx =>
                        {
                    var result = tx.Run("MATCH (start:VoiceA {" +
                        $"name:'VoiceA {i}'" +
                        "}), (end:VoiceA {" +
                        $"name:'VoiceA {j}'" +
                        "}), p"
                        + " = shortestPath((start) -[:connected *]-(end)) RETURN length(p); "
                        );
                    return result.Single()[0].As<string>();
                        }
                    );
                    string message = $"VoiceA {i} and VoiceA {j}";
                    //Console.WriteLine(message + " Shortest path: " + creation);
                    result.TryAdd(message, int.Parse(creation));
                }
            }

            for (int i = 1; i < 7; i++)
            {
                for (int j = i + 1; j <= 7; j++)
                {
                    var creation = session.WriteTransaction(tx =>
                    {
                        var result = tx.Run("MATCH (start:Anime {" +
                            $"name:'Anime {i}'" +
                            "}), (end:Anime {" +
                            $"name:'Anime {j}'" +
                            "}), p"
                            + " = shortestPath((start) -[:connected *]-(end)) RETURN length(p); "
                            );
                        return result.Single()[0].As<string>();
                    }
                    );
                    string message = $"Anime {i} and Anime {j}";
                    //Console.WriteLine(message + " Shortest path: " + creation);
                    result.TryAdd(message, int.Parse(creation));
                }
            }
        }
        return result;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _driver?.Dispose();
        }

        _disposed = true;
    }

    public static void Main()
    {
        using (var creator = new Graph("bolt://localhost:7687/neo4j", "neo4j", "neo4j4"))
        {
            //creator.CreateGraph();
            //creator.CreateRelation(1, 1, "Anime", "VoiceA");
            //creator.CreateRelation(2, 1, "Anime", "VoiceA");
            //creator.CreateRelation(3, 1, "Anime", "VoiceA");
            //creator.CreateRelation(2, 2, "Anime", "VoiceA");
            //creator.CreateRelation(4, 2, "Anime", "VoiceA");
            //creator.CreateRelation(1, 3, "Anime", "VoiceA");
            //creator.CreateRelation(4, 3, "Anime", "VoiceA");
            //creator.CreateRelation(4, 4, "Anime", "VoiceA");
            //creator.CreateRelation(5, 4, "Anime", "VoiceA");
            //creator.CreateRelation(6, 4, "Anime", "VoiceA");
            //creator.CreateRelation(6, 5, "Anime", "VoiceA");
            //creator.CreateRelation(7, 5, "Anime", "VoiceA");

            creator.Created = true;
            Dictionary<string, int> toSort = creator.TestLength();
            var sortedDict = from entry in toSort orderby entry.Value descending select entry;
            foreach (var item in sortedDict)
            {
                Console.WriteLine($"{item.Key} Shortest path: " + item.Value);
            }
        }
    }
}
