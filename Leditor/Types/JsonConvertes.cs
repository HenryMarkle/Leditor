using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Leditor.Types;

public class MultiDimensionalArrayConverter : JsonConverter<int[,,]>
{
    // Deserialization: Converts JSON back into int[,,]
    public override int[,,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token.");
        }

        var tempList = new List<List<List<int>>>(); // Temporary storage for jagged structure

        // Read through each level of nested arrays
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var subList = new List<List<int>>();
                while (reader.Read() && reader.TokenType == JsonTokenType.StartArray)
                {
                    var innerList = new List<int>();
                    while (reader.Read() && reader.TokenType == JsonTokenType.Number)
                    {
                        innerList.Add(reader.GetInt32());
                    }
                    subList.Add(innerList);
                    if (reader.TokenType != JsonTokenType.EndArray)
                    {
                        throw new JsonException("Expected EndArray token.");
                    }
                }
                tempList.Add(subList);
                if (reader.TokenType != JsonTokenType.EndArray)
                {
                    throw new JsonException("Expected EndArray token.");
                }
            }
            else if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }
            else
            {
                throw new JsonException("Expected StartArray or EndArray token.");
            }
        }

        // Convert jagged List<List<List<int>>> into int[,,]
        int depth1 = tempList.Count;
        int depth2 = tempList[0].Count;
        int depth3 = tempList[0][0].Count;

        int[,,] resultArray = new int[depth1, depth2, depth3];

        for (int i = 0; i < depth1; i++)
        {
            for (int j = 0; j < depth2; j++)
            {
                for (int k = 0; k < depth3; k++)
                {
                    resultArray[i, j, k] = tempList[i][j][k];
                }
            }
        }

        return resultArray;
    }

    public override void Write(Utf8JsonWriter writer, int[,,] value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        for (int i = 0; i < value.GetLength(0); i++)
        {
            writer.WriteStartArray();
            for (int j = 0; j < value.GetLength(1); j++)
            {
                writer.WriteStartArray();
                for (int k = 0; k < value.GetLength(2); k++)
                {
                    writer.WriteNumberValue(value[i, j, k]);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }
}

public class Vector2Converter : JsonConverter<Vector2>
{
    // Serialization: Converts Vector2 into JSON
    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }

    // Deserialization: Converts JSON back into Vector2
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token.");
        }

        float x = 0;
        float y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Vector2(x, y);
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString() ?? "";
                reader.Read();

                switch (propertyName)
                {
                    case "X":
                        x = reader.GetSingle();
                        break;
                    case "Y":
                        y = reader.GetSingle();
                        break;
                    default:
                        throw new JsonException($"Unexpected property {propertyName}");
                }
            }
        }

        throw new JsonException("Expected EndObject token.");
    }
}

public class IntTupleConverter : JsonConverter<(int, int)>
{
    // Serialization: Converts (int, int) tuple into JSON array
    public override void Write(Utf8JsonWriter writer, (int, int) value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Item1);
        writer.WriteNumberValue(value.Item2);
        writer.WriteEndArray();
    }

    // Deserialization: Converts JSON array back into (int, int) tuple
    public override (int, int) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token.");
        }

        reader.Read(); // Move to first number
        int item1 = reader.GetInt32();

        reader.Read(); // Move to second number
        int item2 = reader.GetInt32();

        reader.Read(); // Move to EndArray token
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected EndArray token.");
        }

        return (item1, item2);
    }
}