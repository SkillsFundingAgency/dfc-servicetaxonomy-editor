function debouncePreview(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};
function initVueMultiselectPreview(element) {
    // only run script if element exists
    if (element) {
        var elementId = element.id;
        var selectedItems = JSON.parse(element.dataset.selectedItems || "[]");
        var searchUrl = element.dataset.searchUrl;
        var multiple = JSON.parse(element.dataset.multiple);
        var previewCollapse = $('#' + elementId).next('.collapse');
        var previewHere = previewCollapse.find('#previewhere');

        var debouncedSearch = debouncePreview(function (vm, query) {
            vm.isLoading = true;
            var searchFullUrl = searchUrl;
            if (query) {
                searchFullUrl += '&query=' + query;
            }
            fetch(searchFullUrl).then(function (res) {
                res.json().then(function (json) {
                    vm.options = json;
                    vm.isLoading = false;
                })
            });
        }, 250);

        var vueMultiselect = Vue.component('vue-multiselect', window.VueMultiselect.default);

        var vm = new Vue({
            el: '#' + elementId,
            components: { 'vue-multiselect': vueMultiselect },
            data: {
                value: null,
                arrayOfItems: selectedItems,
                options: [],
            },
            computed: {
                selectedIds: function () {
                    return this.arrayOfItems.map(function (x) { return x.id }).join(',');
                },
                isDisabled: function () {
                    return this.arrayOfItems.length > 0 && !multiple;
                }
            },
            watch: {
                selectedIds: function () {
                    // We add a delay to allow for the <input> to get the actual value
                    // before the form is submitted
                    setTimeout(function () { $(document).trigger('contentpreview:render') }, 100);
                    //loop the new order of selected items and make sure the preview UI is ordered correctly
                    for (var i = 0; i < this.arrayOfItems.length; i++) {
                        if (previewHere.children().eq(i).data('content-item-id') !== this.arrayOfItems[i].id) {
                            //find the child which should be at this position
                            var content = previewHere.find('[data-content-item-id=' + this.arrayOfItems[i].id + ']');
                            //insert it before this element
                            content.remove().insertBefore(previewHere.children().eq(i));
                        }
                    }
                }
            },
            mounted: function () {
                var self = this;
                self.asyncFind();
                if (self.arrayOfItems.length > 0) {
                    for (var i = 0; i < self.arrayOfItems.length; i++) {
                        self.showPreview(self.arrayOfItems[i].id, self.arrayOfItems[i].hasPublished);
                    }
                }
            },
            methods: {
                asyncFind: function (query) {
                    var self = this;
                    debouncedSearch(self, query);
                },
                showPreview(contentItemId, hasPublished) {
                    var self = this;

                    // todo use fetch then
                    // we could use the observed selectedIds, but we'd have to calc if something was added or deleted
                    // just use onSelect instead?
                    $.ajax({
                        url : '/Contents/ContentItems/' + contentItemId + (hasPublished?'':'/preview'),
                        type: 'GET',
                        async: false,
                        success: function (data) {
                            //start by removing any previously injected content to handle content picker values changing
                            $('[data-prepend-id]').remove();
                            //parse the view into a temp jQuery DOM tree for manipulation
                            var temp = $('<temp>').append($.parseHTML(data));
                            //find any elements in the view which we want to prepend somewhere else
                            var prependElements = $('[data-prepend-id]', temp);
                            //loop over them, remove them from the original source, and prepend them at their target location
                            prependElements.each(function () {
                                var elem = $(this);
                                var target = elem.data('prepend-id');
                                temp.remove(elem);

                                //timeout required to allow time for the tabs to initialise
                                setTimeout(function () {
                                    $(target).prepend(elem);
                                }, 100);
                            });

                            //inject the remaining content into the preview portion of the page, i.e. remove the <temp> tag we embedded earlier.
                            previewHere.append(temp.children());
                            //add the content item id for the new element so we can use it later
                            previewHere.children().last().attr('data-content-item-id', contentItemId);
                            previewCollapse.collapse('show');
                        }
                    });
                },
                hidePreview() {
                    previewCollapse.collapse('hide');
                    $('[data-prepend-id]').remove();
                },
                onSelect: function (selectedOption, id) {
                    var self = this;

                    for (i = 0; i < self.arrayOfItems.length; i++) {
                        if (self.arrayOfItems[i].id === selectedOption.id) {
                            return;
                        }
                    }

                    self.arrayOfItems.push(selectedOption);
                    this.showPreview(selectedOption.id, selectedOption.hasPublished);
                },
                remove: function (item) {
                    var itemIndex = this.arrayOfItems.indexOf(item);
                    this.arrayOfItems.splice(itemIndex, 1);
                    previewHere.children().eq(itemIndex).remove();

                    if (previewHere.children().length === 0) {
                        this.hidePreview();
                    }
                },
                edit: function (item) {
                    openConfirmModal(this.$refs.editLink.href.replace(encodeURI('<<ID>>'), item.id).concat('?returnUrl=' + window.location.pathname));
                }
            }
        })
    }
}

$(function () {
    $(document).on('click', '.create-button', function (e) {
        e.preventDefault();
        var url = $(this).attr('href').concat('?returnUrl=' + window.location.pathname);
        openConfirmModal(url);
    });

    //TODO : find a better way of doing this? any widget collapsibles that don't contain the preview editor will start closed.
    //maybe we move this to some global JS file at least.
    $('.widget-editor').removeClass('collapsed');
});

function openConfirmModal(url) {
    confirmDialog({
        title: 'Warning',
        message: 'Any unsaved changes on the current page will be lost, are you sure you want to continue?',
        callback: function callback(r) {
            if (r) {
                window.location = url;
            }
        }
    });
}
