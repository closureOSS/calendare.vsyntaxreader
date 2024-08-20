using System;
using System.Collections.Generic;
using Calendare.VSyntaxReader.Models;
using NodaTime;

namespace Calendare.VSyntaxReader.Operations;

public class FreeBusyList : LinkedList<FreeBusyEntry>
{

    public bool Insert(Interval period, FreeBusyStatus status, bool punching = false, int priority = 0)
    {
        if (period.Duration == Duration.Zero)
        {
            return false;    // periods with no duration are ignored
        }
        var current = FindFirstBefore(period.Start);
        while (current is not null && period.End > current.Value.Period.Start)
        {
            var hasHigherStatus = HasHigherStatus(current.Value.Status, status, punching);
            if (priority > current.Value.Priority)
            {
                hasHigherStatus = true;
            }

            if (period.Start <= current.Value.Period.Start)
            {
                // starting at beginning of currentNode (merge left)
                // --> check if previous is same status and than extend previous
                if (current.Previous is not null && current.Previous.Value.Status == status)
                {
                    if (period.End >= current.Value.Period.End)
                    {
                        // if fully covered, drop node and replace with extended previous
                        // B: p -> cur -> n
                        // A: p -> n
                        current.Previous.Value.Period = new Interval(current.Previous.Value.Period.Start, current.Value.Period.End);
                        current = current.Previous;
                        if (current.Next is not null)
                        {
                            Remove(current.Next);
                        }
                    }
                    else
                    {
                        // extend previous and shorten currentNode
                        // B: p -> cur -> n
                        // A: p -> cur -> n
                        current.Previous.Value.Period = new Interval(current.Previous.Value.Period.Start, period.End);
                        current.Value.Period = new Interval(period.End, current.Value.Period.End);
                    }
                }
                else
                {
                    if (hasHigherStatus)
                    {
                        if (period.End >= current.Value.Period.End)
                        {
                            // if fully covered, simply set status and take over
                            // B: p -> cur -> n
                            // A: p -> cur -> n
                            current.Value.Status = status;
                            current.Value.Priority = priority;
                        }
                        else
                        {
                            // otherwise insert new node and shift existing node to the right
                            // B: p -> cur -> n
                            // A: p -> new -> cur -> n
                            AddBefore(current, new FreeBusyEntry
                            {
                                Period = new Interval(current.Value.Period.Start, period.End),
                                Status = status,
                                Priority = priority,
                            });
                            current.Value.Period = new Interval(period.End, current.Value.Period.End);
                        }
                    }
                }
            }
            else if (period.End <= current.Value.Period.End)
            {
                // starting after current node AND ending before end of current node
                if (hasHigherStatus)
                {
                    // B: p -> cur -> n
                    // A: p -> cur -> new -> fix -> n
                    var newNode = AddAfter(current, new FreeBusyEntry
                    {
                        Period = period,
                        Status = status,
                        Priority = priority
                    });
                    AddAfter(newNode, new FreeBusyEntry
                    {
                        Period = new Interval(period.End, current.Value.Period.End),
                        Status = current.Value.Status,
                        Priority = current.Value.Priority,
                    });
                    current.Value.Period = new Interval(current.Value.Period.Start, period.Start);
                    current = newNode;
                }
            }
            else
            {
                // starting after current node AND ending at end or later of current node (merge right)
                if (hasHigherStatus)
                {
                    // B: p -> cur -> n
                    // A: p -> cur -> new -> n
                    var newNode = AddAfter(current, new FreeBusyEntry
                    {
                        Period = new Interval(period.Start, current.Value.Period.End),
                        Status = status,
                        Priority = priority,
                    });
                    current.Value.Period = new Interval(current.Value.Period.Start, period.Start);
                    current = newNode;
                }
            }
            current = current?.Next;
        }
        return true;
    }

    private LinkedListNode<FreeBusyEntry> FindFirstBefore(Instant start)
    {
        var hit = First ?? throw new NullReferenceException($"{nameof(FreeBusyList)} is empty");

        while (hit.Next is not null)
        {
            if (start <= hit.Value.Period.Start || hit.Next is null)
            {
                break;
            }
            hit = hit.Next;
        }
        return start < hit.Value.Period.Start ? (hit.Previous ?? hit) : hit;
    }

    private static bool HasHigherStatus(FreeBusyStatus left, FreeBusyStatus right, bool punching)
    {
        if (left == right)
        {
            return false;
        }
        if (right == FreeBusyStatus.Free && punching)
        {
            return true;
        }
        return left < right;
    }

}
