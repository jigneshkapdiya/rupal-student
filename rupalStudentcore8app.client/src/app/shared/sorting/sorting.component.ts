import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-sorting',
  templateUrl: './sorting.component.html',
  styleUrls: ['./sorting.component.scss']
})
export class SortingComponent implements OnInit {
  @Input() isAscending: boolean;
  @Input() sortBy: any;
  @Input() name: string;
  @Input() title: string;

  constructor() { }

  ngOnInit(): void {
  }

}
