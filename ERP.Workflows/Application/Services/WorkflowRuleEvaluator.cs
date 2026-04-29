using System.Text.Json;

namespace ERP.Workflows.Application.Services;

public sealed class WorkflowRuleEvaluator
{
    public bool Evaluate(string? ruleJson, string? contextJson)
    {
        if (string.IsNullOrWhiteSpace(ruleJson))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(contextJson))
        {
            return false;
        }

        using var ruleDoc = JsonDocument.Parse(ruleJson);
        using var contextDoc = JsonDocument.Parse(contextJson);
        var ruleRoot = ruleDoc.RootElement;
        return EvaluateRule(ruleRoot, contextDoc.RootElement);
    }

    private static bool EvaluateRule(JsonElement ruleElement, JsonElement context)
    {
        if (ruleElement.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (ruleElement.TryGetProperty("all", out var allElement) && allElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var condition in allElement.EnumerateArray())
            {
                if (!EvaluateRule(condition, context))
                {
                    return false;
                }
            }

            return true;
        }

        if (ruleElement.TryGetProperty("any", out var anyElement) && anyElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var condition in anyElement.EnumerateArray())
            {
                if (EvaluateRule(condition, context))
                {
                    return true;
                }
            }

            return false;
        }

        return EvaluateCondition(ruleElement, context);
    }

    private static bool EvaluateCondition(JsonElement condition, JsonElement context)
    {
        if (!condition.TryGetProperty("field", out var fieldElement)
            || !condition.TryGetProperty("op", out var opElement)
            || !condition.TryGetProperty("value", out var valueElement))
        {
            return false;
        }

        var fieldPath = fieldElement.GetString();
        var op = opElement.GetString();
        if (string.IsNullOrWhiteSpace(fieldPath) || string.IsNullOrWhiteSpace(op))
        {
            return false;
        }

        if (!TryResolveField(context, fieldPath, out var fieldValue))
        {
            return false;
        }

        return EvaluateComparison(fieldValue, valueElement, op);
    }

    private static bool TryResolveField(JsonElement context, string fieldPath, out JsonElement fieldValue)
    {
        fieldValue = default;
        var current = context;
        foreach (var segment in fieldPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
            {
                return false;
            }

            current = next;
        }

        fieldValue = current;
        return true;
    }

    private static bool EvaluateComparison(JsonElement left, JsonElement right, string op)
    {
        switch (op)
        {
            case "==":
                return CompareEquality(left, right);
            case "!=":
                return !CompareEquality(left, right);
            case ">":
                return CompareNumbers(left, right, (l, r) => l > r);
            case ">=":
                return CompareNumbers(left, right, (l, r) => l >= r);
            case "<":
                return CompareNumbers(left, right, (l, r) => l < r);
            case "<=":
                return CompareNumbers(left, right, (l, r) => l <= r);
            case "in":
                return CompareIn(left, right);
            case "contains":
                return CompareContains(left, right);
            default:
                return false;
        }
    }

    private static bool CompareEquality(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number)
        {
            return left.GetDecimal() == right.GetDecimal();
        }

        if (left.ValueKind == JsonValueKind.True || left.ValueKind == JsonValueKind.False)
        {
            return left.GetBoolean() == right.GetBoolean();
        }

        return string.Equals(left.ToString(), right.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool CompareNumbers(JsonElement left, JsonElement right, Func<decimal, decimal, bool> comparer)
    {
        if (left.ValueKind != JsonValueKind.Number || right.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        return comparer(left.GetDecimal(), right.GetDecimal());
    }

    private static bool CompareIn(JsonElement left, JsonElement right)
    {
        if (right.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in right.EnumerateArray())
        {
            if (CompareEquality(left, item))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CompareContains(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in left.EnumerateArray())
            {
                if (CompareEquality(item, right))
                {
                    return true;
                }
            }

            return false;
        }

        return left.ToString()?.Contains(right.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true;
    }
}
