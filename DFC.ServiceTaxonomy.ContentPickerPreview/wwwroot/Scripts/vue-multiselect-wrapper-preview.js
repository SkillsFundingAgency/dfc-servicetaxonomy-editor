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
                }
            },
            mounted: function () {
                var self = this;
                self.asyncFind();
                if (self.arrayOfItems.length === 1) {
                    self.showPreview(self.arrayOfItems[0].id);
                }
            },
            methods: {
                asyncFind: function (query) {
                    var self = this;
                    debouncedSearch(self, query);
                },
                showPreview(contentItemId) {
                    var self = this;

                    if (multiple)
                        return;

                    // todo use fetch then
                    // we could use the observed selectedIds, but we'd have to calc if something was added or deleted
                    // just use onSelect instead?
                    $.ajax({
                        url : '/Contents/ContentItems/' + contentItemId,
                        type: 'GET',

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
                            $('#previewhere').html(temp.children());
                            $('#preview-collapse').collapse('show');
                        }
                    });
                },
                hidePreview() {
                    if (multiple)
                        return;

                    $('#preview-collapse').collapse('hide');
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
                },
                onInput: function (value, id) {
                    this.showPreview(value.id);
                },
                remove: function (item) {
                    this.arrayOfItems.splice(this.arrayOfItems.indexOf(item), 1)
                    this.hidePreview();
                }
            }
        })
    }
}
