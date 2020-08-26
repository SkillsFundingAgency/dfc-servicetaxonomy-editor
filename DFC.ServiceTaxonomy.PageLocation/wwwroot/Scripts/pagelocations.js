$(function () {
    if ($('h1:contains("Edit Page Location")').length) {
        var $titleInput = $('input[name="TitlePart.Title"]');
        var $form = $titleInput.closest('form');

        var initialTitle = $titleInput.val();

        $form.on('submit', function (e) {
            e.preventDefault();

            var currentTitle = $titleInput.val();

            if (initialTitle !== currentTitle) {
                confirmDialog({
                    title: 'Warning',
                    message: 'Any associated pages will automatically have their URLs updated to reflect this change, are you sure you want to continue?',
                    callback: function callback(r) {
                        if (r) {
                            $form.unbind().submit();
                        }
                    }
                });
            }
            else {
                $form.unbind().submit();
            }
        });
    }
});
