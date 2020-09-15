$(function () {
    window.slugify = function (string) {
        return string
            .toString()
            .trim()
            .toLowerCase()
            .replace(/\s+/g, "-")
            .replace(/[^\w\-]+/g, "")
            .replace(/\-\-+/g, "-")
            .replace(/^-+/, "")
            .replace(/-+$/, "");
    }

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

    if ($('h1:contains("New Page Location")').length || $('h1:contains("Edit Page Location")').length) {
        var $breadcrumbInput = $('input[name="PageLocation.BreadcrumbText.Text"]');
        var $titleLabel = $('label[for="TitlePart_Title"]');
        var $titleInput = $('input[name="TitlePart.Title"]');
        var $titleHint = $titleInput.siblings('.hint').first();
        var $form = $titleInput.closest('form');

        //title part always takes focus so force the breadcrumb to be focused on page load
        $breadcrumbInput.focus();

        $titleLabel.text('Path');
        $titleHint.text($titleHint.text().replace('title', 'path'));

        $breadcrumbInput.blur(function () {
            if ($titleInput.val().trim().length === 0) {
                $titleInput.val(generatePath($breadcrumbInput.val()));
            }
        });

        $titleInput.blur(function () {
            if ($titleInput.val().trim().length === 0) {
                $titleInput.val(generatePath($breadcrumbInput.val()));
            } else {
                $titleInput.val(generatePath($titleInput.val()));
            }
        });

        $form.on('submit', function () {
            $titleInput.val(generatePath($titleInput.val()));
        });

        function generatePath(value) {
            return (value.startsWith('/') ? '/' : '') + slugify(value);
        }
    }
});
