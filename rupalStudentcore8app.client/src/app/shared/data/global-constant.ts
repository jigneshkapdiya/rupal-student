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

export const StudentSemesterList = [
  { name: "1st Year", nameGU: "પ્રથમ વર્ષ" },
  { name: "2nd Year", nameGU: "બીજું વર્ષ" },
  { name: "3rd Year", nameGU: "ત્રીજું વર્ષ" },
  { name: "4th Year", nameGU: "ચોથું વર્ષ" },
  { name: "1st Semester", nameGU: "પ્રથમ સેમેસ્ટર" },
  { name: "2nd Semester", nameGU: "બીજું સેમેસ્ટર" },
  { name: "3rd Semester", nameGU: "ત્રીજું સેમેસ્ટર" },
  { name: "4th Semester", nameGU: "ચોથું સેમેસ્ટર" },
  { name: "5th Semester", nameGU: "પાંચમું સેમેસ્ટર" },
  { name: "6th Semester", nameGU: "છઠ્ઠું સેમેસ્ટર" },
  { name: "7th Semester", nameGU: "સાતમું સેમેસ્ટર" },
  { name: "8th Semester", nameGU: "આઠમું સેમેસ્ટર" },
  { name: "9th Semester", nameGU: "નવમું સેમેસ્ટર" },
  { name: "10th Semester", nameGU: "દસમું સેમેસ્ટર" },
];


export const StudentStatusList = [
  { name: 'New' },
  { name: 'Approved' },
]





