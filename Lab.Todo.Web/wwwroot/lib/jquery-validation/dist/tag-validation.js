let form = document.querySelector(".form-with-validation");

form.addEventListener('submit', function (event) {
    removeOldErrors();

    if (!checkTagsUniqueness()) {
        event.preventDefault();
    }
});

let removeOldErrors = function () {
    let errors = form.querySelectorAll('.tag-validation-error');
    errors.forEach(error => error.remove());
}

let checkTagsUniqueness = function () {
    let valid = true;
    let tags = document.getElementsByName('TagViewModel.Tags');
    let tagInputs = Array.prototype.filter.call(tags,
        tag => tag.nodeName == 'INPUT');

    let uniqTagValues = [];
    tagInputs.forEach(tag => {
        if (uniqTagValues.indexOf(tag.value) == -1) {
            uniqTagValues.push(tag.value);
        }
        else {
            valid = false;
            tag.classList.add('input-validation-error');

            let error = document.createElement('div');
            error.classList.add('field-validation-error', 'tag-validation-error');
            error.innerHTML = 'Remove duplicate tag - ' + tag.value + '. Tags must be unique';

            let validationSpan = document.getElementById('tags-validation');
            validationSpan.appendChild(error);
        }
    });

    return valid;
}
