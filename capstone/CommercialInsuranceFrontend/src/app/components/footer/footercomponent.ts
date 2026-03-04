import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'app-footer',
    standalone: true,
    imports: [RouterLink],
    templateUrl: './footercomponent.html',
    styleUrl: './footercomponent.css'
})
export class FooterComponent { }

