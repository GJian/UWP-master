﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace MyUWPToolkit
{
    public class GroupObservableCollection<T> : ObservableCollection<T>, ISupportIncrementalLoading, IGroupCollection
    {
        private List<IList<T>> souresList;

        private List<int> firstIndexInEachGroup = new List<int>();
        private List<IGroupHeader> groupHeaders;
        bool _isLoadingMoreItems = false;

        public GroupObservableCollection(List<IList<T>> souresList, List<IGroupHeader> groupHeaders)
        {
            this.souresList = souresList;
            this.groupHeaders = groupHeaders;
        }

        public bool HasMoreItems
        {
            get
            {
                if (CurrentGroupIndex < souresList.Count)
                {
                    var source = souresList[currentGroupIndex];
                    if (source is ISupportIncrementalLoading)
                    {
                        if (!(source as ISupportIncrementalLoading).HasMoreItems)
                        {
                            if (!_isLoadingMoreItems)
                            {
                                groupHeaders[currentGroupIndex].LastIndex = this.Count - 1;
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (CurrentGroupIndex == source.Count - 1)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public List<int> FirstIndexInEachGroup
        {
            get
            {
                return firstIndexInEachGroup;
            }

            set
            {
                firstIndexInEachGroup = value;
            }
        }

        public List<IGroupHeader> GroupHeaders
        {
            get
            {
                return groupHeaders;
            }

            set
            {
                groupHeaders = value;
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return FetchItems(count).AsAsyncOperation();
        }

        private int currentGroupIndex;
        public int CurrentGroupIndex
        {
            get
            {
                int count = 0;

                for (int i = 0; i < souresList.Count; i++)
                {
                    var source = souresList[i];
                    count += source.Count;
                    if (count > this.Count)
                    {
                        currentGroupIndex = i;
                        return currentGroupIndex;
                    }
                    else if (count == this.Count)
                    {
                        if ((source is ISupportIncrementalLoading))
                        {
                            if (!(source as ISupportIncrementalLoading).HasMoreItems)
                            {
                                if (!_isLoadingMoreItems)
                                {
                                    groupHeaders[i].LastIndex = this.Count - 1;
                                    currentGroupIndex = i + 1;
                                }
                                else
                                {
                                    currentGroupIndex = i;
                                }
                            }
                            else
                            {
                                //next
                                currentGroupIndex = i;
                            }
                        }
                        else
                        {
                            //next
                            currentGroupIndex = i + 1;
                        }

                        return currentGroupIndex;
                    }
                    else
                    {
                        continue;
                    }
                }
                currentGroupIndex = 0;
                return currentGroupIndex;
            }
        }

        private async Task<LoadMoreItemsResult> FetchItems(uint count)
        {
            var source = souresList[CurrentGroupIndex];

            if (source is ISupportIncrementalLoading)
            {
                int firstIndex = 0;
                if (groupHeaders[currentGroupIndex].FirstIndex != -1)
                {
                    firstIndex = source.Count;
                }
                _isLoadingMoreItems = true;
                var result = await (source as ISupportIncrementalLoading).LoadMoreItemsAsync(count);

                for (int i = firstIndex; i < source.Count; i++)
                {
                    this.Add(source[i]);
                    if (i == 0)
                    {
                        groupHeaders[currentGroupIndex].FirstIndex = this.Count - 1;
                    }
                }
                _isLoadingMoreItems = false;
                return result;
            }
            else
            {
                int firstIndex = 0;
                if (groupHeaders[currentGroupIndex].FirstIndex != -1)
                {
                    firstIndex = source.Count;
                }
                for (int i = firstIndex; i < source.Count; i++)
                {
                    this.Add(source[i]);
                    if (i == 0)
                    {
                        groupHeaders[currentGroupIndex].FirstIndex = this.Count - 1;
                    }
                }
                groupHeaders[currentGroupIndex].LastIndex = this.Count - 1;

                return new LoadMoreItemsResult() { Count = (uint)source.Count };
            }
        }
    }

}