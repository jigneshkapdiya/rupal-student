import Swal from 'sweetalert2';
export enum eRole {
  Administrator = "Administrator",
  Admin = "Admin",
}

export const PageSize = 20;

export const PageSizeList = [
  { pageSize: 10, name: "10 items per page", nameAr: "عدد السجلات في صفحة" + " 10" },
  { pageSize: 20, name: "20 items per page", nameAr: "عدد السجلات في صفحة" + " 20" },
  { pageSize: 50, name: "50 items per page", nameAr: "عدد السجلات في صفحة" + " 50" },
  { pageSize: 100, name: "100 items per page", nameAr: "عدد السجلات في صفحة" + " 100" },
  { pageSize: 500, name: "500 items per page", nameAr: "عدد السجلات في صفحة" + " 500" },
  { pageSize: 1000, name: "1000 items per page", nameAr: "عدد السجلات في صفحة" + " 1000" },
  { pageSize: 100000, name: "All items", nameAr: "جميع المواد" + " 100000" },
];


export const PatternEmail = "^[a-zA-Z0-9.-_]{1,}@[a-zA-Z.-]{2,}[.]{1}[a-zA-Z]{2,}$";
export const PatternPassword = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[$@$!#%*?&])[A-Za-zd$@$!%*?&].{7,}";
export const PASSWORD_PATTERN = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])[A-Za-z0-9$@$#!%*?&]{8,}$";

export const swalDelete = Swal.mixin({
  title: 'Are you sure?',
  text: "You won't be able to revert this!",
  icon: 'warning',
  showCancelButton: true,
  confirmButtonText: 'Yes, delete it!',
  cancelButtonText: 'Cancel',
  customClass: {
    confirmButton: 'btn btn-info m-1',
    cancelButton: 'btn btn-secondary',
  },
  buttonsStyling: false,
  heightAuto: false,
});

export const StudentShakhList = [
  { name: "Ladola", nameGU: "લાડોલા" },
  { name: "Kapadiya", nameGU: "કાપડિયા" },
  { name: "Chothiya", nameGU: "ચોથિયા" },
  { name: "Kalaiya", nameGU: "કલૈયા" },
  { name: "Pon", nameGU: "પોન" },
  { name: "Regot", nameGU: "રેગોટ" },
  { name: "Malosaniya", nameGU: "માલોસાણિયા" },
  { name: "Other", nameGU: "અન્ય" }
]





